using System;
using System.ComponentModel.DataAnnotations; // Entity Framework

namespace Common.Models
{
    public class TaskBase
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.Medium;

        public DateTime? DueDate { get; set; }

        // TODO : Possible improvement - replacing IsCompleted with a TaskStatusEnum for a more detailed status.
        public bool IsCompleted { get; set; }
    }
}
