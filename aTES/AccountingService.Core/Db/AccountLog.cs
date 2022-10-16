using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountingService.Core.Db
{
    [Table(Name = "ACCOUNT_LOGS")]
    public class AccountLog
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        // should be guid, but I failed to make it work with linq2db yet
        [Column(Name = "PUBLIC_ID")]
        public string PublicId { get; set; }

        [Column(Name = "ACCOUNT_ID")]
        public int AccountId { get; set; }

        [Column(Name = "TASK_ID")]
        public int? TaskId { get; set; }

        [Column(Name = "AMOUNT")]
        public int Amount { get; set; }

        [Column(Name = "CREATED")]
        public DateTime Created { get; set; }
    }
}
