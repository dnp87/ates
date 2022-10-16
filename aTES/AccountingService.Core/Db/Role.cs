using LinqToDB.Mapping;

namespace AccountingService.Core.Db
{
    [Table(Name = "ROLES")]
    public class Role
    {
        [Column(IsPrimaryKey = true, Name = "ID")]
        public int Id { get; set; }

        [Column(Name = "NAME")]
        public string Name { get; set; }
    }
}