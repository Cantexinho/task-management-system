using System;
using System.Collections.Generic;
using System.Linq;
using TaskManagementSys.Api.Dtos.Tasks;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Api.Dtos.Projects
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string StatusName => Status.ToString();
        
        public string? CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        
        public List<TaskSummaryResponse> Tasks { get; set; } = new List<TaskSummaryResponse>();
        
        public bool IsActive => Status == ProjectStatus.Active;
        public bool IsCompleted => Status == ProjectStatus.Completed;
        public double CompletionPercentage => Tasks.Count > 0 
            ? Math.Round((double)Tasks.Count(t => t.IsCompleted) / Tasks.Count * 100, 1) 
            : 0;
    }
    
    public class TaskSummaryResponse
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public string PriorityName => Priority.ToString();
        public TaskItemStatus Status { get; set; }
        public string StatusName => Status.ToString();
        
        public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Today && Status != TaskItemStatus.Completed;
        public bool IsCompleted => Status == TaskItemStatus.Completed;
    }
} 