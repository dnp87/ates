using Common.Constants;
using System;

namespace Common.Events
{
    public class TaskCreatedEventV3 : EventBase<TaskCreatedEventV3Data>
    {
        public override string EventName => EventNames.TaskCreated;
        public override int EventVersion => 3;

        public TaskCreatedEventV3() : base(null)
        {

        }

        public TaskCreatedEventV3(TaskCreatedEventV3Data data) : base(data)
        {
        }

        public TaskCreatedEventV3(DateTime date, TaskCreatedEventV3Data data) : base(date, data)
        {
        }

        public TaskCreatedEventV3(Guid guid, DateTime date, TaskCreatedEventV3Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskCreatedEventV3Data
    {
        public string PublicId { get; set; }
        public string Name { get; set; }
        public string JiraId { get; set; }
        public string Description { get; set; }
        public string ParrotPublicId { get; set; }
    }
}
