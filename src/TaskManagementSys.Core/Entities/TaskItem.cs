using System;

namespace TaskManagementSys.Core.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskItemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
    
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
    
    public enum TaskItemStatus
    {
        Todo,
        InProgress,
        Review,
        Completed,
        Canceled
    }
}