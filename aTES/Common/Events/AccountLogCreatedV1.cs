using Common.Constants;
using System;

namespace Common.Events
{
    public class AccountLogCreatedV1 : EventBase<AccountLogCreatedV1Data>
    {
        public override string EventName => EventNames.AccountLogCreated;

        public override int EventVersion => 1;

        public AccountLogCreatedV1(AccountLogCreatedV1Data data) : base(data)
        {
        }

        public AccountLogCreatedV1(DateTime date, AccountLogCreatedV1Data data) : base(date, data)
        {
        }

        public AccountLogCreatedV1(Guid guid, DateTime date, AccountLogCreatedV1Data data) : base(guid, date, data)
        {
        }

        public AccountLogCreatedV1() : base(null)
        {

        }
    }

    public class AccountLogCreatedV1Data
    {
        public string PublicId { get; set; }
        public string ParrotPublicId { get; set; }
        public string TaskPublicId { get; set; }
        public int Amount { get; set; }
        public DateTime Created { get; set; }
    }
}
