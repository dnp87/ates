using AuthService.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LinqToDB;

namespace AuthService.Controllers
{
    public class ParrotsController : ApiController
    {
        // GET: api/parrots
        [HttpGet]
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
        [HttpGet]
        public Parrot Get(Guid id)
        {            
            using (var db = new AuthDB())
            {
                return db.Parrots.LoadWith(p => p.Role).SingleOrDefault(p => p.PublicId == id);
            }
        }

        // POST: api/parrots
        public void Post([FromBody] Parrot value)
        {
            using (var db = new AuthDB())
            {
                db.Insert(value);
            }
        }

        // PUT: api/Parrots/5
        public void Put(int id, [FromBody] Parrot value)
        {
            using (var db = new AuthDB())
            {
                db.InsertOrReplace(value);
            }
        }

        // DELETE: api/Parrots/5
        public void Delete(int id)
        {
            using (var db = new AuthDB())
            {
                db.Parrots.Delete(p => p.Id == id);
            }
        }
    }
}
