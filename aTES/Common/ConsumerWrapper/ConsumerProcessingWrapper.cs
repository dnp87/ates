using Common.Constants;
using Confluent.Kafka;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Threading;

namespace Common.ConsumerWrapper
{
    // todo: interface, di, etc...
    public static class ConsumerProcessingWrapper
    {
        public static void ContiniouslyConsume<T>(string topic, Action<T> action)
        {
            var conf = new ConsumerConfig
            {
                GroupId = ConfigurationManager.AppSettings[ConfigurationKeys.ConsumerGroupId],
                BootstrapServers = ConfigurationManager.AppSettings[ConfigurationKeys.KafkaBootstrapServers],
            };
            using (var builder = new ConsumerBuilder<string, string>(conf).Build())
            {
                builder.Subscribe(topic);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    while (true)
                    {
                        var consumeResult = builder.Consume(cancelToken.Token);
                        var typedEvent = JsonConvert.DeserializeObject<T>(consumeResult.Message.Value);
                        action(typedEvent);
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
