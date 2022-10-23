using AccountingService.Core.Db;
using Common.Auth;
using Common.Constants;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AccountingService.Controllers
{
    [ServiceAuth]
    public class AccountController : ApiController
    {
        // GET: api/Account/mine
        [HttpGet]
        [Route("api/Account/mine")]
        public Account Mine()
        {
            // todo: implement it as correct claims
            string publicId = ((GenericIdentityWithPublicId)RequestContext.Principal.Identity).PublicId;
            return Get(publicId);
        }

        // GET: api/Account/5
        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Accountant)]
        public Account Get(string id)
        {
            using (var db = new AccountingDB())
            {
                return db.Accounts.LoadWith(a => a.AccountLog).FirstOrDefault(a => a.PublicId == id);
            }
        }

        [Authorize(Roles = RoleNames.Administrator + "," + RoleNames.Accountant)]
        [HttpGet]
        [Route("api/Account/profit")]
        public int Profit(DateTime? date)
        {
            date = date ?? DateTime.Today;

            // select account transaction related to tasks by current date
            using (var db = new AccountingDB())
            {
                var q = from al in db.AccountLogs
                        where al.TaskId != null && al.Created.Date == date.Value.Date
                        select -al.Amount; // parrot's profit -> company's loss
                return q.Sum();
            }
        }
    }
}
