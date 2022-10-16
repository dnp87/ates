using AccountingService.Core.Db;
using Common.Constants;
using Common.ConsumerWrapper;
using Common.Events;
using Common.Utils;
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
                            var task = new Core.Db.Task
                            {
                                PublicId = typedEvent.Data.PublicId,
                                Description = typedEvent.Data.Description,
                                Name = pair.name,
                                JiraId = pair.jiraId,
                                ParrotId = typedEvent.Data.ParrotId,
                                AssignedAmount = typedEvent.Data.AssignedAmount,
                                CompletedAmount = typedEvent.Data.CompletedAmount,
                            };

                            db.Insert(task);
                        }
                    }
                });            
        }

        static void ConsumeTaskCreatedV2Topic()
        {

        }

        static void ConsumeTaskAssignedTopic()
        {

        }

        static void ConsumeTaskCompletedTopic()
        {

        }
    }
}
