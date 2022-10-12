using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ParrotLoginModel
    {
        public bool LoginSuccessful { get; set; }

        public string Name { get; set; }

        public string PublicId { get; set; }

        public string[] Roles { get; set; }
    }
}
