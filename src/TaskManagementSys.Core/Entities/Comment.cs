using System;

namespace TaskManagementSys.Core.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskItemId { get; set; }
        public required string UserId { get; set; }
        
        // Navigation properties
        public virtual TaskItem? TaskItem { get; set; }
    }
}