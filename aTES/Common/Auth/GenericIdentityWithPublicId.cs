using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Common.Auth
{
    public class GenericIdentityWithPublicId : GenericIdentity
    {
        public string PublicId { get; private set; }
        
        public GenericIdentityWithPublicId(string name, string publicId) : base(name)
        {
            PublicId = publicId;
        }

        public GenericIdentityWithPublicId(string name, string publicId, string type) : base(name, type)
        {
            PublicId = publicId;
        }
    }
}
