using LinqToDB;

namespace AccountingService.Core.Db
{
    public class AccountingDB : LinqToDB.Data.DataConnection
    {
        public AccountingDB() : base("AccountingDB")
        {
        }

        public ITable<Role> Roles => this.GetTable<Role>();
        public ITable<Parrot> Parrots => this.GetTable<Parrot>();
        public ITable<Task> Tasks => this.GetTable<Task>();
        public ITable<Account> Accounts => this.GetTable<Account>();
        public ITable<AccountLog> AccountLogs => this.GetTable<AccountLog>();
    }
}