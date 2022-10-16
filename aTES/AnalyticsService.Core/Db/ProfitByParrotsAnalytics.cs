using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsService.Core.Db
{
    [Table(Name = "PROFIT_BY_PARROT_ANALYTICS")]
    public class ProfitByParrotsAnalytics
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        [Column(Name = "PUBLIC_ID")]
        public string PublicId { get; set; }

        //PARROT_ID INTEGER NOT NULL REFERENCES PARROTS(ID),
        //PROFIT_AMOUNT INTEGER DEFAULT(0) NOT NULL,  -- in a real system this would be a decimal
        //PROFIT_DATE DATE NOT NULL
    }
}
