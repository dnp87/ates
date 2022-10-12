using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthService.Models
{
    public class ParrotPostPutModel
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }
    }
}