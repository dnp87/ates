using AnalyticsService.Core.Db;
using Common.Constants;
using Common.Events;
using Confluent.Kafka;
using LinqToDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var conf = new ConsumerConfig
            {
                GroupId = "st_consumer_group",
                BootstrapServers = "localhost:9092",
            };
            using (var builder = new ConsumerBuilder<string, string>(conf).Build())
            {
                builder.Subscribe(TopicNames.ParrotCreatedV2);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    while (true)
                    {
                        var consumeResult = builder.Consume(cancelToken.Token);
                        var createdEvent = JsonConvert.DeserializeObject<ParrotCreatedEventV2>(consumeResult.Message.Value);
                        using (var db = new AnalyticsDB())
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
                catch (Exception e)
                {
                    builder.Close();
                }
            }
        }

        static void ConsumeParrotUpdatedTopic()
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
                        var updatedEvent = JsonConvert.DeserializeObject<ParrotUpdatedEventV1>(consumeResult.Message.Value);
                        using (var db = new AnalyticsDB())
                        {
                            var parrot = db.Parrots.FirstOrDefault(p => p.PublicId == updatedEvent.Data.PublicId.ToString());
                            parrot.Email = updatedEvent.Data.Email;
                            parrot.Name = updatedEvent.Data.Name;
                            parrot.PublicId = updatedEvent.Data.PublicId;
                            parrot.RoleId = (int)updatedEvent.Data.RoleId;

                            db.Update(parrot);
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (OperationCanceledException)
                {
                    // consume timeout, no-op
                }
                catch (Exception e)
                {
                    builder.Close();
                }
            }
        }
    }
}
