using Common.Constants;
using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Threading;
using TaskTrackerService.Db;
using LinqToDB;
using Common.Events;
using System.Linq;
using System.Configuration;
using Common.ConsumerWrapper;

namespace TaskTrackerService
{
    public class TopicConsumer
    {        
        public void ConsumeParrotCreatedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.ParrotCreatedV2,
                (ParrotCreatedEventV2 typedEvent) =>
                {
                    using (var db = new TaskTrackerDB())
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

        public void ConsumeParrotUpdatedTopic()
        {
            ConsumerProcessingWrapper.ContiniouslyConsume(TopicNames.ParrotUpdatedV1,
                (ParrotUpdatedEventV1 typedEvent) =>
                {
                    using (var db = new TaskTrackerDB())
                    {
                        var parrot = db.Parrots.FirstOrDefault(p => p.PublicId == typedEvent.Data.PublicId.ToString());
                        if(parrot != null)
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