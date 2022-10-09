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

namespace TaskTrackerService.Controllers
{
    [ServiceAuth]
    public class TasksController : ApiController
    {
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
            //todo: get current parrot id
            using (var db = new TaskTrackerDB())
            {
                var q = from t in db.Tasks
                        where t.ParrotId == 0 //todo
                        select t;
                return q.LoadWith(t => t.Parrot).ToArray();
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
        public void Post([FromBody]TaskPostModel postModel)
        {
            using (var db = new TaskTrackerDB())
            {
                int parrotId = GetRandomEngineerParrotId();

                db.Insert(new Task
                {
                    PublicId = Guid.NewGuid().ToString(),
                    ParrotId = parrotId,
                    Name = postModel.Name,
                    Description = postModel.Description,
                    Status = Common.Enums.TaskStatus.Active,
                });
            }

            //TODO: events
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
