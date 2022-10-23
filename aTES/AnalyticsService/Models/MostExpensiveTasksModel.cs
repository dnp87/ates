using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnalyticsService.Models
{
    public class MostExpensiveTasksModel
    {
        public MostExpensiveTask Monthly { get; set; }
        public MostExpensiveTask Weekly { get; set; }
        public List<MostExpensiveTask> Daily { get; set; }
    }

    public class MostExpensiveTask
    {
        public string Name { get; set; }
        public int Amount { get; set; }

        public string Period { get; set; }
    }
}