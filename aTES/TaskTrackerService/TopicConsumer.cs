using Common.Constants;
using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using TaskTrackerService.Db;
using LinqToDB;

namespace TaskTrackerService
{
    public class TopicConsumer
    {
        public void ConsumeTopics()
        {
            var conf = new ConsumerConfig
            {
                GroupId = "st_consumer_group",
                BootstrapServers = "localhost:9092",
            };
            using (var builder = new ConsumerBuilder<string,string>(conf).Build())
            {
                builder.Subscribe(TopicNames.ParrotCreatedV1);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    while (true)
                    {
                        var consumeResult = builder.Consume(cancelToken.Token);
                        var parrot = JsonConvert.DeserializeObject<Parrot>(consumeResult.Message.Value);
                        using(var db = new TaskTrackerDB())
                        {
                            db.Insert(parrot);
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception)
                {
                    builder.Close();
                }
            }            
        }
    }
}