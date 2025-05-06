using System;
using System.Collections.Generic;

namespace TaskManagementSys.Core.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public required string CreatedByUserId { get; set; }
        
        // Navigation properties
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
    
    public enum ProjectStatus
    {
        Planning,
        Active,
        OnHold,
        Completed,
        Canceled
    }
}