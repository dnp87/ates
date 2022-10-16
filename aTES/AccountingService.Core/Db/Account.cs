using LinqToDB.Mapping;

namespace AccountingService.Core.Db
{
    [Table(Name = "ACCOUNTS")]
    public class Account
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        // should be guid, but I failed to make it work with linq2db yet
        [Column(Name = "PUBLIC_ID")]
        public string PublicId { get; set; }

        [Column(Name = "PARROT_ID")]
        public int ParrotId { get; set; }
        
        [Column(Name = "AMOUNT")]
        public int Amount { get; set; }
    }
}
