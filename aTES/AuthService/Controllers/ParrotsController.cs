using AuthService.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LinqToDB;
using AuthService.Models;

namespace AuthService.Controllers
{
    public class ParrotsController : ApiController
    {
        // GET: api/parrots
        public IEnumerable<Parrot> Get()
        {
            using (var db = new AuthDB())
            {                
                var q = from p in db.Parrots
                        select p;
                return q.LoadWith(p => p.Role).ToArray();                
            }
        }

        // GET: api/parrots/5
        public Parrot Get(Guid id)
        {            
            using (var db = new AuthDB())
            {
                return db.Parrots.LoadWith(p => p.Role).SingleOrDefault(p => p.PublicId == id.ToString());
            }
        }

        // POST: api/parrots
        public void Post([FromBody] ParrotPostPutModel value)
        {
            using (var db = new AuthDB())
            {
                db.Insert(new Parrot
                {
                    PublicId = Guid.NewGuid().ToString(),
                    Name = value.Name,
                    Email = value.Email,
                    RoleId = value.RoleId,
                });
            }
        }

        // PUT: api/Parrots/...
        public void Put(Guid id, [FromBody] ParrotPostPutModel value)
        {
            using (var db = new AuthDB())
            {
                // todo: remove custom get
                var parrot = db.Parrots.FirstOrDefault(p => p.PublicId == id.ToString());
                if (parrot == null)
                {
                    db.Insert(new Parrot
                    {
                        PublicId = id.ToString(),
                        Name = value.Name,
                        Email = value.Email,
                        RoleId = value.RoleId,
                    });
                }
                else
                {
                    parrot.Name = value.Name;
                    parrot.Email = value.Email;
                    parrot.RoleId = value.RoleId;
                    db.Update(parrot);
                }
            }
        }

        // no parrot deletion
    }
}
