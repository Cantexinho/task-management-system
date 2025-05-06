using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Core.Services;
using TaskManagementSys.Infrastructure.Repositories;

namespace TaskManagementSys.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        private readonly TaskRepository _taskRepository;

        public TaskService(TaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _taskRepository.GetAllTasksAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _taskRepository.GetTaskByIdAsync(id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            task.CreatedAt = DateTime.UtcNow;
            
            if (task.Status == TaskItemStatus.Completed && !task.CompletedAt.HasValue)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            
            return await _taskRepository.CreateTaskAsync(task);
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            var existingTask = await _taskRepository.GetTaskByIdAsync(task.Id);
            if (existingTask == null)
            {
                throw new ArgumentException($"Task with ID {task.Id} not found");
            }
            
            if (task.Status == TaskItemStatus.Completed && !task.CompletedAt.HasValue)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            
            if (existingTask.Status == TaskItemStatus.Completed && task.Status != TaskItemStatus.Completed)
            {
                task.CompletedAt = null;
            }
            
            return await _taskRepository.UpdateTaskAsync(task);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            return await _taskRepository.DeleteTaskAsync(id);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }
            
            return await _taskRepository.GetTasksByUserIdAsync(userId);
        }
        
        public async Task<TaskAssignment> AssignTaskAsync(TaskAssignment assignment)
        {
            if (assignment == null)
            {
                throw new ArgumentNullException(nameof(assignment));
            }
            
            if (string.IsNullOrEmpty(assignment.UserId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }
            
            var task = await _taskRepository.GetTaskByIdAsync(assignment.TaskItemId);
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {assignment.TaskItemId} not found");
            }
            
            await _taskRepository.DeactivateTaskAssignmentsAsync(assignment.TaskItemId, assignment.UserId);
            
            if (assignment.AssignedAt == default)
            {
                assignment.AssignedAt = DateTime.UtcNow;
            }
            
            assignment.IsActive = true;
            
            return await _taskRepository.CreateTaskAssignmentAsync(assignment);
        }
        
        public async Task<bool> UnassignTaskAsync(int taskId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }
            
            return await _taskRepository.DeactivateTaskAssignmentsAsync(taskId, userId);
        }
    }
}