using Common.Constants;
using System;

namespace Common.Events
{
    public class TaskAssignedEventV2 : EventBase<TaskAssignedEventV2Data>
    {
        public override string EventName => EventNames.TaskAssigned;
        public override int EventVersion => 2;

        public TaskAssignedEventV2() : base(null)
        {

        }

        public TaskAssignedEventV2(TaskAssignedEventV2Data data) : base(data)
        {
        }

        public TaskAssignedEventV2(DateTime date, TaskAssignedEventV2Data data) : base(date, data)
        {
        }

        public TaskAssignedEventV2(Guid guid, DateTime date, TaskAssignedEventV2Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskAssignedEventV2Data
    {
        public string TaskPublicId { get; set; }
        public string ParrotPublicId { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
