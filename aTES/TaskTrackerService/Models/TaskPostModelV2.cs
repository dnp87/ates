using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskTrackerService.Models
{
    public class TaskPostModelV2 : TaskPostModelV1
    {
        [Required]
        public string JiraId { get; set; }
    }
}