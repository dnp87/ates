using Common.Constants;
using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Threading;
using TaskTrackerService.Db;
using LinqToDB;
using Common.Events;

namespace TaskTrackerService
{
    public class TopicConsumer
    {
        public void ConsumeParrotCreatedTopic()
        {
            var conf = new ConsumerConfig
            {
                GroupId = "st_consumer_group",
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
                GroupId = "st_consumer_group",
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
                        var createdEvent = JsonConvert.DeserializeObject<ParrotUpdatedEventV1>(consumeResult.Message.Value);
                        using (var db = new TaskTrackerDB())
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