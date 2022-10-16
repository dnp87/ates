using Common.Constants;
using System;

namespace Common.Events
{
    public class TaskAssignedEventV1 : EventBase<TaskAssignedEventV1Data>
    {
        public override string EventName => EventNames.TaskAssigned;
        public override int EventVersion => 1;

        public TaskAssignedEventV1() : base(null)
        {

        }

        public TaskAssignedEventV1(TaskAssignedEventV1Data data) : base(data)
        {
        }

        public TaskAssignedEventV1(DateTime date, TaskAssignedEventV1Data data) : base(date, data)
        {
        }

        public TaskAssignedEventV1(Guid guid, DateTime date, TaskAssignedEventV1Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskAssignedEventV1Data
    {
        public string TaskPublicId { get; set; }
        public string ParrotPublicId { get; set; }
        public DateTime AssignedDate { get; set; }
        public int AssingedAmount { get; set; }
    }
}
