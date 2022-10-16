using Common.Constants;
using System;

namespace Common.Events
{
    public class TaskCreatedEventV1 : EventBase<TaskCreatedEventV1Data>
    {
        public override string EventName => EventNames.TaskCreated;
        public override int EventVersion => 1;

        public TaskCreatedEventV1(TaskCreatedEventV1Data data) : base(data)
        {
        }

        public TaskCreatedEventV1(DateTime date, TaskCreatedEventV1Data data) : base(date, data)
        {
        }

        public TaskCreatedEventV1(Guid guid, DateTime date, TaskCreatedEventV1Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskCreatedEventV1Data
    {
        public string PublicId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParrotPublicId { get; set; }
        public int AssignedAmount { get; set; }
        public int CompletedAmount { get; set; }
    }
}
