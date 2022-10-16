using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalyticsService.Core.Db
{
    [Table(Name = "PARROTS")]
    public class Parrot
    {
        [PrimaryKey]
        [Identity]
        [Column(Name = "ID")]
        public int Id { get; set; }

        // should be guid, but I failed to make it work with linq2db yet
        [Column(Name = "PUBLIC_ID")]
        public string PublicId { get; set; }

        [Column(Name = "NAME")]
        public string Name { get; set; }

        [Column(Name = "EMAIL")]
        public string Email { get; set; }

        [Column(Name = "ROLE_ID")]
        public int RoleId { get; set; }

        [Association(ThisKey = nameof(RoleId), OtherKey = nameof(Db.Role.Id))]
        public Role Role { get; set; }
    }
}