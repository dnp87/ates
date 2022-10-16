﻿using Common.Constants;
using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Threading;
using TaskTrackerService.Db;
using LinqToDB;
using Common.Events;
using System.Linq;

namespace TaskTrackerService
{
    public class TopicConsumer
    {
        private const string ConsumerGroupName = "task_tracker_service_consumer_group";

        public void ConsumeParrotCreatedTopic()
        {
            var conf = new ConsumerConfig
            {
                GroupId = ConsumerGroupName,
                BootstrapServers = "localhost:9092",
            };
            using (var builder = new ConsumerBuilder<string,string>(conf).Build())
            {
                builder.Subscribe(TopicNames.ParrotCreatedV2);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    while (true)
                    {                        
                        var consumeResult = builder.Consume(cancelToken.Token);
                        var createdEvent = JsonConvert.DeserializeObject<ParrotCreatedEventV2>(consumeResult.Message.Value);
                        using(var db = new TaskTrackerDB())
                        {
                            db.Insert(new Parrot
                            {
                                Email = createdEvent.Data.Email,
                                Name = createdEvent.Data.Name,
                                PublicId = createdEvent.Data.PublicId,
                                RoleId = (int)createdEvent.Data.RoleId
                            });
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch(OperationCanceledException)
                {
                    // consume timeout, no-op
                }
                catch (Exception)
                {
                    builder.Close();
                }
            }            
        }

        public void ConsumeParrotUpdatedTopic()
        {
            var conf = new ConsumerConfig
            {
                GroupId = ConsumerGroupName,
                BootstrapServers = "localhost:9092",
            };
            using (var builder = new ConsumerBuilder<string, string>(conf).Build())
            {
                builder.Subscribe(TopicNames.ParrotUpdatedV1);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    while (true)
                    {
                        var consumeResult = builder.Consume(cancelToken.Token);
                        var updatedEvent = JsonConvert.DeserializeObject<ParrotUpdatedEventV1>(consumeResult.Message.Value);
                        using (var db = new TaskTrackerDB())
                        {
                            var parrot = db.Parrots.FirstOrDefault(p => p.PublicId == updatedEvent.Data.PublicId.ToString());
                            parrot.Email = updatedEvent.Data.Email;
                            parrot.Name = updatedEvent.Data.Name;
                            parrot.PublicId = updatedEvent.Data.PublicId;
                            parrot.RoleId = (int) updatedEvent.Data.RoleId;

                            db.Update(parrot);
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (OperationCanceledException)
                {
                    // consume timeout, no-op
                }
                catch (Exception)
                {
                    builder.Close();
                }
            }
        }
    }
}