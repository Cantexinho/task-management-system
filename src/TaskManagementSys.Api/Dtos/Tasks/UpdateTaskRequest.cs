using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Api.Dtos.Tasks
{
    public class UpdateTaskRequest
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string? Title { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
        
        [Required]
        [EnumDataType(typeof(TaskPriority))]
        public TaskPriority Priority { get; set; }
        
        public int? ProjectId { get; set; }
        
        [Required]
        [EnumDataType(typeof(TaskItemStatus))]
        public TaskItemStatus Status { get; set; }
        
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
} 