using Common.Constants;
using Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Events
{
    public class ParrotCreatedEventV2 : EventBase<ParrotCreatedEventV2Data>
    {
        public override string EventName => EventNames.ParrotCreated;
        public override int EventVersion => 2;

        public ParrotCreatedEventV2() : base(null)
        {

        }

        public ParrotCreatedEventV2(ParrotCreatedEventV2Data data) : base(data)
        {
        }

        public ParrotCreatedEventV2(DateTime date, ParrotCreatedEventV2Data data) : base(date, data)
        {
        }
    }

    public class ParrotCreatedEventV2Data
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PublicId { get; set; }

        public RoleIds RoleId { get; set; }
    }
}
