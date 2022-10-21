using Common.Events;
using Common.SchemaRegistry;
using Confluent.Kafka;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.ProducerWrapper
{
    public class ProducerWrapper : IProducerWrapper
    {
        private ISchemaValidator _schemaValidator;

        public ProducerWrapper(ISchemaValidator schemaValidator)
        {
            _schemaValidator = schemaValidator;
        }

        public bool TrySendMessage<T>(IProducer<string, string> producer, string topicName, string key, T message, out IList<string> errors)
            where T : EventBase
        {
            try
            {
                string json = JsonConvert.SerializeObject(message);
                if (_schemaValidator.ValidateBySchema(json, message.EventName, message.EventVersion, out errors))
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
            catch (System.Exception e)
            {
                errors = new List<string> { e.Message };
                return false;
            }
        }
    }
}
