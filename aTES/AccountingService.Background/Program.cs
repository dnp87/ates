using AccountingService.Core.Db;
using Common.Constants;
using Common.ConsumerWrapper;
using Common.Events;
using Common.Pricing;
using Common.Utils;
using Hangfire;
using Hangfire.MemoryStorage;
using LinqToDB;
using System;
using System.Linq;

namespace AccountingService.Background
{
    internal class Program
    {
        private static ITaskPricing _taskPricing = new TaskPricing();
        private const int MaxDbReadAttempts = 10;

        static void Main(string[] args)
        {
            Hangfire.GlobalConfiguration.Configuration.UseStorage(
                new MemoryStorage());
            RecurringJob.AddOrUpdate(() => ResetPositiveBalance(), Cron.Daily);

            System.Threading.Tasks.Task.WhenAll(
                System.Threading.Tasks.Task.Run(ConsumeParrotCreatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeParrotUpdatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCreatedV3Topic),
                ConsumeTaskAssignedTopic(),
                System.Threading.Tasks.Task.Run(ConsumeTaskCompletedTopic)
                ).Wait();
        }

        public static void ResetPositiveBalance()
        {
            //todo
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
            var task = db.Tasks.First(a => a.PublicId == taskPublicId);

            var accountLog = new AccountLog
            {
                AccountId = account.Id,
                TaskId = task.Id,
                Amount = amount,
                Created = date,
                PublicId = Guid.NewGuid().ToString(),
            };
            db.Insert(accountLog);
        }

        static async System.Threading.Tasks.Task ConsumeTaskAssignedTopic()
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
                                await System.Threading.Tasks.Task.Delay(500);
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
