using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TaskTrackerService.Db;
using LinqToDB;
using TaskTrackerService.Models;
using Common.Enums;
using Common.Auth;
using Common.Constants;
using Common.Pricing;
using System.Text.RegularExpressions;
using Common.Utils;
using Confluent.Kafka;
using Common.ProducerWrapper;
using System.Configuration;
using Common.SchemaRegistry;
using Common.Events;

namespace TaskTrackerService.Controllers
{
    [ServiceAuth]
    public class TasksController : ApiController
    {        
        private IProducer<string, string> _parrotCreateProducer;
        private IProducerWrapper _producerWrapper;

        public TasksController() : base()
        {
            var conf = new ProducerConfig()
            {
                BootstrapServers = ConfigurationManager.AppSettings[ConfigurationKeys.KafkaBootstrapServers],
            };
            var producer = new ProducerBuilder<string, string>(conf);
            _parrotCreateProducer = producer.Build();

            _producerWrapper = new ProducerWrapper(new SchemaValidator());

        }

        // GET: api/Tasks
        public IEnumerable<Task> Get()
        {
            using (var db = new TaskTrackerDB())
            {
                var q = from t in db.Tasks
                        select t;
                return q.LoadWith(t => t.Parrot).ToArray();
            }
        }

        // GET: api/Tasks/mine
        [Route("api/Tasks/mine")]
        [HttpGet]
        public IEnumerable<Task> Mine()
        {
            // todo: implement it as correct claims
            string publicId = ((GenericIdentityWithPublicId)RequestContext.Principal.Identity).PublicId;
            int parrotId = GetParrotIdByPublicId(publicId);

            using (var db = new TaskTrackerDB())
            {
                var q = from t in db.Tasks
                        where t.ParrotId == parrotId
                        select t;
                return q.LoadWith(t => t.Parrot).ToArray();
            }
        }

        private int GetParrotIdByPublicId(string publicId)
        {
            using (var db = new TaskTrackerDB())
            {
                var q = from p in db.Parrots
                        where p.PublicId == publicId
                        select p.Id;
                return q.First();
            }
        }

        // GET: api/Tasks/5
        public Task Get(Guid id)
        {
            using (var db = new TaskTrackerDB())
            {
                return db.Tasks.LoadWith(t => t.Parrot).SingleOrDefault(p => p.PublicId == id.ToString());
            }
        }

        // POST: api/Tasks
        public HttpResponseMessage Post([FromBody]TaskPostModelV3 postModel)
        {
            using (var db = new TaskTrackerDB())
            {
                using(var tran = db.BeginTransaction())
                {
                    var parrot = GetRandomEngineerParrot();

                    var task = new Task
                    {
                        PublicId = Guid.NewGuid().ToString(),
                        ParrotId = parrot.Id,
                        Name = postModel.Name,
                        JiraId = postModel.JiraId,
                        Description = postModel.Description,
                        Status = Common.Enums.TaskStatus.Active
                    };
                    db.Insert(task);

                    bool sent1 = _producerWrapper.TrySendMessage(
                    _parrotCreateProducer, TopicNames.TaskCreatedV3, task.PublicId,
                    new TaskCreatedEventV3(new TaskCreatedEventV3Data
                    {
                        PublicId = task.PublicId,
                        ParrotPublicId = parrot.PublicId,
                        Name = task.Name,
                        JiraId = task.JiraId,
                        Description = task.Description,
                    }), out IList<string> errors1);

                    IList<string> errors2 = new List<string>();
                    bool sent2 = sent1 && _producerWrapper.TrySendMessage(
                        _parrotCreateProducer, TopicNames.TaskAssignedV2, Guid.NewGuid().ToString(),
                        new TaskAssignedEventV2(new TaskAssignedEventV2Data
                        {
                            TaskPublicId = task.PublicId,
                            ParrotPublicId = parrot.PublicId,
                        }), out errors2);

                    if (sent2)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        ModelState.AddModelError("Topic", String.Join("; ", errors1.Concat(errors2)));
                    }
                }
            }

            if (ModelState.IsValid)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // PUT: api/Tasks/Complete/{public_id}
        [Route("api/Tasks/Complete/{id}")]
        [HttpPut]
        public HttpResponseMessage Complete(Guid id)
        {
            using (var db = new TaskTrackerDB())
            {
                db.BeginTransaction();
                var task = db.Tasks.LoadWith(t => t.Parrot).FirstOrDefault(t => t.PublicId == id.ToString());
                task.Status = TaskStatus.Completed;
                task.DateCompleted = DateTime.Now;
                db.Update(task);

                bool sent = _producerWrapper.TrySendMessage(
                        _parrotCreateProducer, TopicNames.TaskCompletedV2, Guid.NewGuid().ToString(),
                        new TaskCompletedEventV2(new TaskCompletedEventV2Data
                        {
                            CompletedDate = task.DateCompleted,
                            ParrotPublicId = task.Parrot.PublicId,
                            TaskPublicId = task.PublicId
                        }), out IList<string> errors);
                
                if (sent)
                {
                    db.CommitTransaction();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    db.RollbackTransaction();
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
        }

        // POST: api/shuffle
        [Authorize(Roles = RoleNames.Manager + "," + RoleNames.Administrator)]
        [Route("api/Tasks/shuffle")]
        [HttpPost]
        public void Shuffle()
        {
            using (var db = new TaskTrackerDB())
            {
                var tasks = db.Tasks.Where(t => t.Status == Common.Enums.TaskStatus.Active).ToArray();
                foreach(var task in tasks)
                {
                    var parrot = GetRandomEngineerParrot();
                    if(parrot.Id != task.ParrotId)
                    {
                        db.BeginTransaction();
                        task.ParrotId = parrot.Id;
                        db.Update(task);

                        bool sent = _producerWrapper.TrySendMessage(
                        _parrotCreateProducer, TopicNames.TaskAssignedV2, Guid.NewGuid().ToString(),
                        new TaskAssignedEventV2(new TaskAssignedEventV2Data
                        {                            
                            TaskPublicId = task.PublicId,
                            ParrotPublicId = parrot.PublicId,                        
                        }), out IList<string> errors);

                        if (sent)
                        {
                            db.CommitTransaction();
                        }
                        else
                        {
                            db.RollbackTransaction();
                        }
                    }
                }
            }
        }

        private Parrot GetRandomEngineerParrot()
        {
            using (var db = new TaskTrackerDB())
            {
                var parrots = db.Parrots
                    .Where(p => p.RoleId == (int)RoleIds.Engineer)
                    .ToArray();
                // todo: replace with skip-take
                var idx = new Random().Next(parrots.Length);
                return parrots[idx];
            }
        }

        // no task deletion
    }
}
