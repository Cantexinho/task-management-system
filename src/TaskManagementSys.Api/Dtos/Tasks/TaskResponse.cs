using System;
using System.Collections.Generic;
using TaskManagementSys.Api.Dtos.Projects;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Api.Dtos.Tasks
{
    public class TaskResponse
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public string PriorityName => Priority.ToString();
        public TaskItemStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public int? ProjectId { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        
        public ProjectSummaryResponse? Project { get; set; }
        public List<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();
        public List<TaskAssignmentResponse> Assignments { get; set; } = new List<TaskAssignmentResponse>();
        
        public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && Status != TaskItemStatus.Completed;
        public bool IsCompleted => Status == TaskItemStatus.Completed;
    }
    
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
    }
    
    public class TaskAssignmentResponse
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }
} 