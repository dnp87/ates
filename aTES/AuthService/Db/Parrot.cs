using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthService.Db
{
    [Table(Name = "PARROTS")]
    public class Parrot
    {
        [Column(IsPrimaryKey = true, Name = "ID")]
        public int Id { get; set; }

        [Column(Name = "PUBLIC_ID")]
        public Guid PublicId { get; set; }

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