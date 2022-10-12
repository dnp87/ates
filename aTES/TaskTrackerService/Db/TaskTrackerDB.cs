using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskTrackerService.Db
{
    public class TaskTrackerDB : LinqToDB.Data.DataConnection
    {
        public TaskTrackerDB() : base(nameof(TaskTrackerDB))
        {
        }

        public ITable<Role> Roles => this.GetTable<Role>();
        public ITable<Parrot> Parrots => this.GetTable<Parrot>();
        public ITable<Task> Tasks => this.GetTable<Task>();
    }
}