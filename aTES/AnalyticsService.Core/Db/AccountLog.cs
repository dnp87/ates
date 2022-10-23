using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsService.Core.Db
{
    [Table(Name = "ACCOUNT_LOGS")]
    public class AccountLog
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        [Column(Name = "PUBLIC_ID")]
        public string PublicId { get; set; }
        
        [Column(Name = "PARROT_ID")]
        public int ParrotId { get; set; }

        [Column(Name = "TASK_ID")]
        public int? TaskId { get; set; }

        [Column(Name = "AMOUNT")]
        public int Amount { get; set; }

        [Column(Name = "CREATED")]
        public DateTime Created { get; set; }
    }
}
