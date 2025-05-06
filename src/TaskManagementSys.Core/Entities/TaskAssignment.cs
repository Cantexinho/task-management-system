using System;

namespace TaskManagementSys.Core.Entities
{
    public class TaskAssignment
    {
        public int Id { get; set; }
        public int TaskItemId { get; set; }
        public required string UserId { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }

        public required string AssignedById { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        
        // Navigation properties
        public TaskItem? TaskItem { get; set; }
    }
}