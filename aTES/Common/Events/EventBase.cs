using System;

namespace Common.Events
{
    public abstract class EventBase<T> : EventBase
    {
        public EventBase(Guid guid, DateTime date, T data) : base(guid, date)
        {
            Data = data;
        }

        public EventBase(DateTime date, T data) : this(Guid.NewGuid(), date, data)
        {
            Data = data;
        }

        public EventBase(T data) : this(Guid.NewGuid(), DateTime.Now, data)
        {
        }

        public T Data { get; set; }
    }

    public abstract class EventBase
    {
        public EventBase(Guid guid, DateTime date)
        {
            EventId = guid;
            EventDate = date;
        }

        public EventBase() : this(Guid.NewGuid(), DateTime.Now)
        {
        }

        public Guid EventId { get; private set; } = Guid.NewGuid();        
        public DateTime EventDate { get; set; }
        public abstract string EventName { get; }
        public abstract int EventVersion { get; }
    }
}
