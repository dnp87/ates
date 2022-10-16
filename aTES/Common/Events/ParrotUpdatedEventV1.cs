using Common.Constants;
using Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Events
{
    public class ParrotUpdatedEventV1 : EventBase<ParrotUpdatedEventV1Data>
    {
        public override string EventName => EventNames.ParrotUpdated;
        public override int EventVersion => 1;

        public ParrotUpdatedEventV1() : base(null)
        {

        }

        public ParrotUpdatedEventV1(ParrotUpdatedEventV1Data data) : base(data)
        {
        }

        public ParrotUpdatedEventV1(DateTime date, ParrotUpdatedEventV1Data data) : base(date, data)
        {
        }
    }

    public class ParrotUpdatedEventV1Data
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
