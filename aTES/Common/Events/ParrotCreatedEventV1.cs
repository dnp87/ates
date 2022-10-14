using Common.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Events
{
    public class ParrotCreatedEventV1 : EventBase<ParrotCreatedEventV1Data>
    {
        public override string EventName => EventNames.ParrotCreated;
        public override int EventVersion => 1;

        public ParrotCreatedEventV1(ParrotCreatedEventV1Data data) : base(data)
        {
        }

        public ParrotCreatedEventV1(DateTime date, ParrotCreatedEventV1Data data) : base(date, data)
        {
        }
    }

    public class ParrotCreatedEventV1Data
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public int RoleId { get; set; }
    }
}
