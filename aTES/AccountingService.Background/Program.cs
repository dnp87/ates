using AccountingService.Core.Db;
using Common.Constants;
using Common.ConsumerWrapper;
using Common.Events;
using Common.Pricing;
using Common.ProducerWrapper;
using Common.SchemaRegistry;
using Common.Utils;
using Confluent.Kafka;
using Hangfire;
using Hangfire.MemoryStorage;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace AccountingService.Background
{
    internal class Program
    {
        private static ITaskPricing _taskPricing = new TaskPricing();
        private const int MaxDbReadAttempts = 10;

        private static IProducer<string, string> _producer;
        private static IProducerWrapper _producerWrapper;

        static void Main(string[] args)
        {
            var conf = new ProducerConfig()
            {
                BootstrapServers = ConfigurationManager.AppSettings[ConfigurationKeys.KafkaBootstrapServers],
            };
            var producer = new ProducerBuilder<string, string>(conf);
            _producer = producer.Build();
            _producerWrapper = new ProducerWrapper(new SchemaValidator());


            Hangfire.GlobalConfiguration.Configuration.UseStorage(
                new MemoryStorage());
            RecurringJob.AddOrUpdate(() => ResetPositiveBalance(), Cron.Daily);

            System.Threading.Tasks.Task.WhenAll(
                System.Threading.Tasks.Task.Run(ConsumeParrotCreatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeParrotUpdatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCreatedV3Topic),
                System.Threading.Tasks.Task.Run(ConsumeTaskAssignedTopic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCompletedTopic)
                ).Wait();
        }

        public static void ResetPositiveBalance()
        {
            var date = DateTime.Today;
            using(var db = new AccountingDB())
            {
                using (var tran = db.BeginTransaction())
                {
                    var q = from t in db.AccountLogs
                            join a in db.Accounts on t.AccountId equals a.Id
                            join p in db.Parrots on a.ParrotId equals p.Id
                            group t by p.PublicId into pGroup
                            select new
                            {
                                Sum = pGroup.Sum(o => o.Amount),
                                ParrotPublicId = pGroup.Key
                            };
                    q = q.Where(o => o.Sum > 0);
                    var accountsToBeReset = q.ToArray();

                    foreach (var account in accountsToBeReset)
                    {
                        CreateTaskAccountLogRecord(db, account.ParrotPublicId, null, -account.Sum, date);
                    }

                    tran.Commit();
                }
            }
        }

        static void ConsumeParrotCreatedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.ParrotCreatedV2,
                (ParrotCreatedEventV2 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        using(var tran = db.BeginTransaction())
                        {
                            int parrotId = db.InsertWithInt32Identity(new Parrot
                            {
                                Email = typedEvent.Data.Email,
                                Name = typedEvent.Data.Name,
                                PublicId = typedEvent.Data.PublicId,
                                RoleId = (int)typedEvent.Data.RoleId
                            });

                            db.Insert(new Account
                            {
                                ParrotId = parrotId,
                                PublicId = Guid.NewGuid().ToString(),
                            });

                            db.CommitTransaction();
                        }
                    }
                });
        }        

        static void ConsumeParrotUpdatedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.ParrotUpdatedV1,
                (ParrotUpdatedEventV1 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        var parrot = db.Parrots.FirstOrDefault(p => p.PublicId == typedEvent.Data.PublicId.ToString());
                        parrot.Email = typedEvent.Data.Email;
                        parrot.Name = typedEvent.Data.Name;
                        parrot.PublicId = typedEvent.Data.PublicId;
                        parrot.RoleId = (int)typedEvent.Data.RoleId;

                        db.Update(parrot);
                    }
                });
        }

        static void ConsumeTaskCreatedV3Topic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskCreatedV3,
                (TaskCreatedEventV3 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        db.BeginTransaction();

                        var parrot = db.Parrots.First(p => p.PublicId == typedEvent.Data.ParrotPublicId);
                        var account = db.Accounts.First(a => a.ParrotId == parrot.Id);

                        var task = new Core.Db.Task
                        {
                            PublicId = typedEvent.Data.PublicId,
                            Description = typedEvent.Data.Description,
                            Name = typedEvent.Data.Name,
                            JiraId = typedEvent.Data.JiraId,
                            ParrotId = parrot.Id,
                            AssignedAmount = _taskPricing.GetAssignAmount(),
                            CompletedAmount = _taskPricing.GetCompletedAmount(),
                            Status = Common.Enums.TaskStatus.Active
                        };

                        int taskId = db.InsertWithInt32Identity(task);

                        db.CommitTransaction();
                    }
                });
        }

        private static void CreateTaskAccountLogRecord(AccountingDB db, string parrotPublicId, string taskPublicId, int amount, DateTime date)
        {
            var parrot = db.Parrots.First(p => p.PublicId == parrotPublicId);
            var account = db.Accounts.First(a => a.ParrotId == parrot.Id);
            var task = !String.IsNullOrEmpty(taskPublicId) 
                ? db.Tasks.First(a => a.PublicId == taskPublicId)
                : (Task) null;

            var accountLog = new AccountLog
            {
                AccountId = account.Id,
                TaskId = task?.Id,
                Amount = amount,
                Created = date,
                PublicId = Guid.NewGuid().ToString(),
            };
            db.Insert(accountLog);

            // todo: correct error handling
            _producerWrapper.TrySendMessage(_producer, TopicNames.AccountLogCreatedV1, accountLog.PublicId,
                new AccountLogCreatedV1(new AccountLogCreatedV1Data
                {
                    PublicId = accountLog.PublicId,
                    ParrotPublicId = parrotPublicId,
                    TaskPublicId = taskPublicId,
                    Amount = amount,
                    Created = date,                    
                }), out IList<string> errors);
        }

        static void ConsumeTaskAssignedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskAssignedV2,
                async (TaskAssignedEventV2 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        Task task = null;
                        // trying to get a task up to MaxDbReadAttempts times
                        int tryCount = 0;
                        while (task == null && tryCount < MaxDbReadAttempts)
                        {
                            task = db.Tasks.FirstOrDefault(a => a.PublicId == typedEvent.Data.TaskPublicId);
                            if(task == null)
                            {
                                System.Threading.Tasks.Task.Delay(500).Wait();
                            }
                            tryCount++;
                        }
                        // if we can't get our task from db in MaxDbReadAttempts attemps
                        // we'd fail here.
                        // So I need to learn how to do it properly
                        CreateTaskAccountLogRecord(db, typedEvent.Data.ParrotPublicId, typedEvent.Data.TaskPublicId, -task.AssignedAmount, typedEvent.EventDate);
                    }
                });
        }

        static void ConsumeTaskCompletedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskCompletedV2,
                (TaskCompletedEventV2 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        var task = db.Tasks.First(a => a.PublicId == typedEvent.Data.TaskPublicId);
                        CreateTaskAccountLogRecord(db, typedEvent.Data.ParrotPublicId, typedEvent.Data.TaskPublicId, task.CompletedAmount, typedEvent.EventDate);
                    }
                });
        }
    }
}
