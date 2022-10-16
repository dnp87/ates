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
            Task.WhenAll(
                System.Threading.Tasks.Task.Run(ConsumeParrotCreatedTopic),
                System.Threading.Tasks.Task.Run(ConsumeParrotUpdatedTopic)
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
    }
}
