using System.ComponentModel.DataAnnotations;

namespace TaskTrackerService.Models
{
    public class TaskPostModelV1
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}