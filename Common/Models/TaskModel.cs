using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Models
{
    public class TaskModel : TaskBase
    {
        [StringLength(500)]
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }
}