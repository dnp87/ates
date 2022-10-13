using Common.Events;
using Confluent.Kafka;

namespace Common.ProducerWrapper
{
    public interface IProducerWrapper
    {
        bool TrySendMessage<T>(IProducer<string, string> producer, string topicName, string key, T message)
            where T : EventBase;
    }
}
