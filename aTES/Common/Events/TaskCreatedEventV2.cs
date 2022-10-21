using Common.Constants;
using System;

namespace Common.Events
{
    public class TaskCreatedEventV2 : EventBase<TaskCreatedEventV2Data>
    {
        public override string EventName => EventNames.TaskCreated;
        public override int EventVersion => 2;

        public TaskCreatedEventV2() : base(null)
        {

        }

        public TaskCreatedEventV2(TaskCreatedEventV2Data data) : base(data)
        {
        }

        public TaskCreatedEventV2(DateTime date, TaskCreatedEventV2Data data) : base(date, data)
        {
        }

        public TaskCreatedEventV2(Guid guid, DateTime date, TaskCreatedEventV2Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskCreatedEventV2Data
    {
        public string PublicId { get; set; }
        public string Name { get; set; }
        public string JiraId { get; set; }
        public string Description { get; set; }
        public string ParrotPublicId { get; set; }
        public int AssignedAmount { get; set; }
        public int CompletedAmount { get; set; }
    }
}
