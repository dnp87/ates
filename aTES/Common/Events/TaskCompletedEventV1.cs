using Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class TaskCompletedEventV1 : EventBase<TaskCompletedEventV1Data>
    {
        public override string EventName => EventNames.TaskCompleted;
        public override int EventVersion => 1;

        public TaskCompletedEventV1(TaskCompletedEventV1Data data) : base(data)
        {
        }

        public TaskCompletedEventV1(DateTime date, TaskCompletedEventV1Data data) : base(date, data)
        {
        }

        public TaskCompletedEventV1(Guid guid, DateTime date, TaskCompletedEventV1Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskCompletedEventV1Data
    {
        public string TaskPublicId { get; set; }
        public string ParrotPublicId { get; set; }
        public DateTime CompletedDate { get; set; }
        public int CompletedAmount { get; set; }
    }
}
