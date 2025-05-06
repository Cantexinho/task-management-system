namespace TaskManagementSys.Core.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskItemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Foreign keys
        public int? ProjectId { get; set; }
        public required string CreatedByUserId { get; set; }
        
        // Navigation properties
        public Project? Project { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
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