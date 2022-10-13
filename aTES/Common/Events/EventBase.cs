using System;

namespace Common.Events
{
    public class EventBase<T> : EventBase
    {
        public EventBase(string name, int version, DateTime date, T data) : base(name, version, date)
        {
            Data = data;
        }

        public EventBase(string name, int version, T data) : this(name, version, DateTime.Now, data)
        {
        }

        public T Data { get; set; }
    }

    public class EventBase
    {
        public EventBase(string name, int version, DateTime date)
        {
            EventVersion = version;
            EventName = name;
            EventDate = date;
        }

        public EventBase(string name, int version) : this(name, version, DateTime.Now)
        {
        }

        public Guid EventId { get; } = Guid.NewGuid();
        public int EventVersion { get; set; }
        public DateTime EventDate { get; set; }
        public string EventName { get; set; }
    }
}
