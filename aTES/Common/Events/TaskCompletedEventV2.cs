using Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Events
{
    public class TaskCompletedEventV2 : EventBase<TaskCompletedEventV2Data>
    {
        public override string EventName => EventNames.TaskCompleted;
        public override int EventVersion => 2;

        public TaskCompletedEventV2() : base(null)
        {

        }

        public TaskCompletedEventV2(TaskCompletedEventV2Data data) : base(data)
        {
        }

        public TaskCompletedEventV2(DateTime date, TaskCompletedEventV2Data data) : base(date, data)
        {
        }

        public TaskCompletedEventV2(Guid guid, DateTime date, TaskCompletedEventV2Data data) : base(guid, date, data)
        {
        }
    }

    public class TaskCompletedEventV2Data
    {
        public string TaskPublicId { get; set; }
        public string ParrotPublicId { get; set; }
        public DateTime CompletedDate { get; set; }
    }
}
