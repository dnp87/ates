using AnalyticsService.Core.Db;
using AnalyticsService.Models;
using Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace AnalyticsService.Controllers
{
    [Authorize(Roles = RoleNames.Administrator)]
    public class AnalyticsController : ApiController
    {
        // copy-paste from accounting
        [HttpGet]
        [Route("api/Analytics/profit")]
        public int Profit(DateTime? date)
        {
            date = date ?? DateTime.Today;

            // select account transaction related to tasks by current date
            using (var db = new AnalyticsDB())
            {
                var q = from al in db.AccountLogs
                        where al.TaskId != null && al.Created.Date == date.Value.Date
                        select -al.Amount; // parrot's profit -> company's loss
                return q.Sum();
            }
        }

        [HttpGet]
        public MostExpensiveTasksModel MostExpensiveTasks()
        {
            return MostExpensiveTasks(DateTime.Today);
        }

        [HttpGet]
        public MostExpensiveTasksModel MostExpensiveTasks(DateTime date)
        {
            DateTime monthDateBegin = new DateTime(date.Year, date.Month, 1);
            DateTime monthDateEnd = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            DateTime weekDateBegin = StartOfWeek(date.Date, DayOfWeek.Monday); //todo: use culture
            DateTime weekDateEnd = EndOfWeek(date.Date, DayOfWeek.Sunday); //todo: use culture

            List<DateTime> monthDates = new List<DateTime>();
            for (int i = 1; i <= DateTime.DaysInMonth(date.Year, date.Month); i++)
            {
                monthDates.Add(new DateTime(date.Year, date.Month, i));
            }

            return new MostExpensiveTasksModel
            {
                Monthly = GetMostExpensiveTask(monthDateBegin, monthDateEnd),
                Weekly = GetMostExpensiveTask(weekDateBegin, weekDateEnd),
                Daily = monthDates.Select(o => GetMostExpensiveTask(o, o)).Where(o => o != null).ToList()
            };
        }

        /// <summary>
        /// Get most expensive task by date period.
        /// Ex: getting most expensive task for today would be
        /// var today = DateTime.Today
        /// GetMostExpensiveTask(today, today)
        /// </summary>
        /// <param name="dateBeginInclusive">begin date (time is ignored)</param>
        /// <param name="dateEndExclusive">end date (time is ignored)</param>
        /// <returns>record of most expensive task, null if there weren't any tasks</returns>
        private MostExpensiveTask GetMostExpensiveTask(DateTime dateBeginInclusive, DateTime dateEndExclusive)
        {
            using (var db = new AnalyticsDB())
            {
                var q = from al in db.AccountLogs
                        join t in db.Tasks on al.TaskId equals t.Id
                        where al.TaskId != null
                        && al.Created >= dateBeginInclusive.Date
                        && al.Created < dateEndExclusive.Date.AddDays(1)
                        && al.Amount > 0
                        select new
                        {
                            TaskId = al.TaskId,
                            Name = t.Name,
                            Amount = al.Amount
                        };
                if (q.Count() > 0)
                {
                    var maxAmount = q.Max(al => al.Amount);
                    var record = q.Where(al => al.Amount == maxAmount).First();

                    var met = new MostExpensiveTask
                    {
                        Amount = maxAmount,
                        Name = record.Name,
                    };
                    if (dateBeginInclusive.Date == dateEndExclusive.Date)
                    {
                        met.Period = dateBeginInclusive.Date.ToShortDateString();
                    }
                    else
                    {
                        met.Period = $"{dateBeginInclusive.Date.ToShortDateString()}-{dateEndExclusive.Date.ToShortDateString()}";
                    }

                    return met;
                }
                else
                {
                    return null;
                }
            }
        }

        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private static DateTime EndOfWeek(DateTime dt, DayOfWeek endOfWeek)
        {
            int diff = (7 + (endOfWeek - dt.DayOfWeek)) % 7;
            return dt.AddDays(diff).Date;
        }

    }
}
