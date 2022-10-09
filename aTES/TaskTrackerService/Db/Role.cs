using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LinqToDB.Mapping;

namespace TaskTrackerService.Db
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