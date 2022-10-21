using Common.Events;
using Confluent.Kafka;
using System.Collections.Generic;

namespace Common.ProducerWrapper
{
    public interface IProducerWrapper
    {
        bool TrySendMessage<T>(IProducer<string, string> producer, string topicName, string key, T message, out IList<string> errors)
            where T : EventBase;
    }
}
