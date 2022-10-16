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

namespace TaskTrackerService.Controllers
{
    [ServiceAuth]
    public class TasksController : ApiController
    {
        private readonly ITaskPricing _taskPricing;

        public TasksController() : base()
        {
            //todo: dependency injection
            _taskPricing = new TaskPricing();

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
        public HttpResponseMessage Post([FromBody]TaskPostModelV2 postModel)
        {
            using (var db = new TaskTrackerDB())
            {
                using(var tran = db.BeginTransaction())
                {
                    int parrotId = GetRandomEngineerParrotId();

                    db.Insert(new Task
                    {
                        PublicId = Guid.NewGuid().ToString(),
                        ParrotId = parrotId,
                        Name = postModel.Name,
                        JiraId = postModel.JiraId,
                        Description = postModel.Description,
                        Status = Common.Enums.TaskStatus.Active,
                        AssignedAmount = _taskPricing.GetAssignAmount(),
                        CompletedAmount = _taskPricing.GetCompletedAmount(),
                    });

                    //TODO: events
                    tran.Commit();
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

        // POST: api/Tasks/v1
        [HttpPost]
        [Route("api/Tasks/v1")]
        public HttpResponseMessage Postv1([FromBody] TaskPostModelV1 postModel)
        {            
            if(!TryParseJiraIdAndName(postModel.Name, out (string jiraId, string name) pair))
            {
                ModelState.AddModelError(nameof(postModel.Name), "Can't extract jira id and name from input string");
            }

            if (ModelState.IsValid)
            {
                using (var db = new TaskTrackerDB())
                {
                    using(var tran = db.BeginTransaction())
                    {
                        int parrotId = GetRandomEngineerParrotId();

                        db.Insert(new Task
                        {
                            PublicId = Guid.NewGuid().ToString(),
                            ParrotId = parrotId,
                            Name = pair.name,
                            JiraId = pair.jiraId,
                            Description = postModel.Description,
                            Status = Common.Enums.TaskStatus.Active,
                            AssignedAmount = _taskPricing.GetAssignAmount(),
                            CompletedAmount = _taskPricing.GetCompletedAmount(),
                        });

                        //TODO: events
                        tran.Commit();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        private bool TryParseJiraIdAndName(string name, out (string jiraId, string name) pair)
        {
            var regex = new Regex(@"\[(.+)\]\s+[\-]\s+(.*)");
            var match = regex.Match(name);
            if(match.Success)
            {
                pair = (match.Groups[0].Value, match.Groups[1].Value);
                return false;
            }
            else
            {
                pair = (String.Empty, name);
                return false;
            }
        }

        // PUT: api/Tasks/Complete/{public_id}
        [Route("api/Tasks/Complete/{id}")]
        [HttpPut]
        public void Complete(Guid id)
        {
            using (var db = new TaskTrackerDB())
            {
                db.Tasks
                    .Where(p => p.PublicId == id.ToString())
                    .Set(t => t.Status, TaskStatus.Completed)
                    .Update();
            }

            //TODO: events
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
                    int randomParrotId = GetRandomEngineerParrotId();
                    if(randomParrotId != task.ParrotId)
                    {
                        task.ParrotId = randomParrotId;
                        db.Update(task);

                        // TODO: event for task
                    }
                }
            }            
        }

        private int GetRandomEngineerParrotId()
        {
            using (var db = new TaskTrackerDB())
            {
                var publicIds = db.Parrots
                    .Where(p => p.RoleId == (int)RoleIds.Engineer)
                    .Select(p => p.Id)
                    .ToArray();
                // todo: replace with skip-take
                var idx = new Random().Next(publicIds.Length);
                return publicIds[idx];
            }
        }        

        // no task deletion
    }
}
