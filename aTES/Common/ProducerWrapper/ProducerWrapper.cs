using Common.Events;
using Common.SchemaRegistry;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Common.ProducerWrapper
{
    public class ProducerWrapper : IProducerWrapper
    {
        private ISchemaValidator _schemaValidator;

        public ProducerWrapper(ISchemaValidator schemaValidator)
        {
            _schemaValidator = schemaValidator;
        }

        public bool TrySendMessage<T>(IProducer<string, string> producer, string topicName, string key, T message)
            where T : EventBase
        {
            try
            {
                string json = JsonConvert.SerializeObject(message);
                if (_schemaValidator.ValidateBySchema(json, message.EventName, message.EventVersion))
                {
                    producer.Produce(topicName, new Message<string, string>
                    {
                        Key = key,
                        Value = json,
                    });
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //todo: log
                return false;
            }
        }
    }
}
