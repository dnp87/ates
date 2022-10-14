using AuthService.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LinqToDB;
using AuthService.Models;
using Confluent.Kafka;
using Common.Constants;
using Common.ProducerWrapper;
using Common.SchemaRegistry;
using Common.Events;
using Common.Enums;

namespace AuthService.Controllers
{
    public class ParrotsController : ApiController
    {
        private IProducer<string, string> _parrotCreateProducer;
        private IProducerWrapper _producerWrapper;

        public ParrotsController() : base()
        {
            var conf = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092",
            };
            var producer = new ProducerBuilder<string, string>(conf);
            _parrotCreateProducer = producer.Build();

            _producerWrapper = new ProducerWrapper(new SchemaValidator());
        }

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
        public HttpResponseMessage Post([FromBody] ParrotPostPutModel value)
        {
            Parrot newParrot;
            using (var db = new AuthDB())
            {
                using (var tran = db.BeginTransaction())
                {
                    newParrot = new Parrot
                    {
                        PublicId = Guid.NewGuid().ToString(),
                        Name = value.Name,
                        Email = value.Email,
                        RoleId = value.RoleId,
                    };
                    db.Insert(newParrot);

                    bool sent = _producerWrapper.TrySendMessage(
                    _parrotCreateProducer, TopicNames.ParrotCreatedV2, newParrot.PublicId,
                    new ParrotCreatedEventV2(new ParrotCreatedEventV2Data
                    {
                        Name = value.Name,
                        Email = value.Email,
                        RoleId = (RoleIds) value.RoleId,
                        PublicId = newParrot.PublicId,
                    }));
                    
                    if(sent)
                    {
                        db.CommitTransaction();
                    }
                    else
                    {
                        ModelState.AddModelError("Topic", "failed to sent topic");
                    }    
                }

                if (ModelState.IsValid)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
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

        // todo: events
    }
}
