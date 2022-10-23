using AnalyticsService.Core.Db;
using Common.Constants;
using Common.ConsumerWrapper;
using Common.Events;
using Confluent.Kafka;
using LinqToDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyticsService.Background
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Tasks.Task.WhenAll(
                System.Threading.Tasks.Task.Run(ConsumeParrotCreatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeParrotUpdatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeTaskCreatedV3Topic),
                System.Threading.Tasks.Task.Run(ConsumeAccountLogCreatedV1Topic)
                ).Wait();
        }

        static void ConsumeParrotCreatedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.ParrotCreatedV2,
                (ParrotCreatedEventV2 typedEvent) =>
                {
                    using (var db = new AnalyticsDB())
                    {
                        db.Insert(new Parrot
                        {
                            Email = typedEvent.Data.Email,
                            Name = typedEvent.Data.Name,
                            PublicId = typedEvent.Data.PublicId,
                            RoleId = (int)typedEvent.Data.RoleId
                        });
                    }
                });
        }

        static void ConsumeParrotUpdatedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.ParrotUpdatedV1,
                (ParrotUpdatedEventV1 typedEvent) =>
                {
                    using (var db = new AnalyticsDB())
                    {
                        var parrot = db.Parrots.FirstOrDefault(p => p.PublicId == typedEvent.Data.PublicId.ToString());
                        if (parrot != null)
                        {
                            parrot.Email = typedEvent.Data.Email;
                            parrot.Name = typedEvent.Data.Name;
                            parrot.PublicId = typedEvent.Data.PublicId;
                            parrot.RoleId = (int)typedEvent.Data.RoleId;

                            db.Update(parrot);
                        }
                    }
                });
        }

        static void ConsumeTaskCreatedV3Topic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.TaskCreatedV3,
                (TaskCreatedEventV3 typedEvent) =>
                {
                    using (var db = new AnalyticsDB())
                    {
                        db.BeginTransaction();

                        var parrot = db.Parrots.First(p => p.PublicId == typedEvent.Data.ParrotPublicId);

                        var task = new Core.Db.Task
                        {
                            PublicId = typedEvent.Data.PublicId,
                            Description = typedEvent.Data.Description,
                            Name = typedEvent.Data.Name,
                            JiraId = typedEvent.Data.JiraId,
                            ParrotId = parrot.Id,
                        };

                        int taskId = db.InsertWithInt32Identity(task);

                        db.CommitTransaction();
                    }
                });
        }

        static void ConsumeAccountLogCreatedV1Topic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.AccountLogCreatedV1,
                (AccountLogCreatedV1 typedEvent) =>
                {
                    using (var db = new AnalyticsDB())
                    {
                        db.BeginTransaction();

                        var parrot = db.Parrots.First(p => p.PublicId == typedEvent.Data.ParrotPublicId);
                        var task = !String.IsNullOrEmpty(typedEvent.Data.TaskPublicId)
                            ? db.Tasks.First(p => p.PublicId == typedEvent.Data.TaskPublicId)
                            : null;

                        var accountLog = new Core.Db.AccountLog
                        {
                            PublicId = typedEvent.Data.PublicId,
                            ParrotId = parrot.Id,
                            TaskId = task?.Id,
                            Amount = typedEvent.Data.Amount,
                            Created = typedEvent.Data.Created
                        };

                        db.InsertWithInt32Identity(accountLog);

                        db.CommitTransaction();
                    }
                });
        }
    }
}
