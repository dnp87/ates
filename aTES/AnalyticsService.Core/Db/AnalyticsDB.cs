using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalyticsService.Core.Db
{
    public class AnalyticsDB : LinqToDB.Data.DataConnection
    {
        public AnalyticsDB() : base("AnalyticsDB")
        {
        }

        public ITable<Role> Roles => this.GetTable<Role>();
        public ITable<Parrot> Parrots  => this.GetTable<Parrot>();
        public ITable<ProfitByParrotsAnalytics> ProfitByParrotsAnalytics => this.GetTable<ProfitByParrotsAnalytics>();
        public ITable<TaskAnalytics> TaskAnalytics => this.GetTable<TaskAnalytics>();
    }
}