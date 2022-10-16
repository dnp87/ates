using AccountingService.Core.Db;
using Common.Constants;
using Common.ConsumerWrapper;
using Common.Events;
using Common.Utils;
using LinqToDB;
using System;
using System.Linq;

namespace AccountingService.Background
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Tasks.Task.WhenAll(
                System.Threading.Tasks.Task.Run(ConsumeParrotCreatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeParrotUpdatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCreatedV1Topic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCreatedV2Topic),
                System.Threading.Tasks.Task.Run(ConsumeTaskAssignedTopic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCompletedTopic)
                ).Wait();
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

        static void ConsumeTaskCreatedV1Topic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskCreatedV1,
                (TaskCreatedEventV1 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        if (TaskNameParser.TryParseJiraIdAndName(typedEvent.Data.Name, out (string jiraId, string name) pair))
                        {
                            db.BeginTransaction();
                            var parrot = db.Parrots.First(p => p.PublicId == typedEvent.Data.ParrotPublicId);
                            var account = db.Accounts.First(a => a.ParrotId == parrot.Id);

                            var task = new Core.Db.Task
                            {
                                PublicId = typedEvent.Data.PublicId,
                                Description = typedEvent.Data.Description,
                                Name = pair.name,
                                JiraId = pair.jiraId,
                                ParrotId = parrot.Id,
                                AssignedAmount = typedEvent.Data.AssignedAmount,
                                CompletedAmount = typedEvent.Data.CompletedAmount,
                            };

                            int taskId = db.InsertWithInt32Identity(task);
                            CreateTaskAccountLogRecord(db, typedEvent.Data.ParrotPublicId, typedEvent.Data.PublicId, -typedEvent.Data.AssignedAmount);
                            db.CommitTransaction();
                        }
                    }
                });
        }

        static void ConsumeTaskCreatedV2Topic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskCreatedV2,
                (TaskCreatedEventV2 typedEvent) =>
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
                            AssignedAmount = typedEvent.Data.AssignedAmount,
                            CompletedAmount = typedEvent.Data.CompletedAmount,
                        };

                        int taskId = db.InsertWithInt32Identity(task);
                        CreateTaskAccountLogRecord(db, typedEvent.Data.ParrotPublicId, typedEvent.Data.PublicId, -typedEvent.Data.AssignedAmount);

                        db.CommitTransaction();
                    }
                });
        }

        private static void CreateTaskAccountLogRecord(AccountingDB db, string parrotPublicId, string taskPublicId, int amount)
        {
            var parrot = db.Parrots.First(p => p.PublicId == parrotPublicId);
            var account = db.Accounts.First(a => a.ParrotId == parrot.Id);
            var task = db.Tasks.First(a => a.PublicId == taskPublicId);

            var accountLog = new AccountLog
            {
                AccountId = account.Id,
                TaskId = task.Id,
                Amount = amount,
                Created = DateTime.Now, //not for prod
                PublicId = Guid.NewGuid().ToString(),
            };
            db.Insert(accountLog);
        }

        static void ConsumeTaskAssignedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskAssignedV1,
                (TaskAssignedEventV1 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        CreateTaskAccountLogRecord(db, typedEvent.Data.ParrotPublicId, typedEvent.Data.TaskPublicId, -typedEvent.Data.AssingedAmount);
                    }
                });
        }

        static void ConsumeTaskCompletedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskCompletedV1,
                (TaskCompletedEventV1 typedEvent) =>
                {
                    using (var db = new AccountingDB())
                    {
                        CreateTaskAccountLogRecord(db, typedEvent.Data.ParrotPublicId, typedEvent.Data.TaskPublicId, typedEvent.Data.CompletedAmount);
                    }
                });
        }
    }
}
