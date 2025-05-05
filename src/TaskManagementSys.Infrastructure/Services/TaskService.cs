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

        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            return await _taskRepository.GetTaskByIdAsync(id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            task.CreatedAt = DateTime.Now;
            
            if (task.Status == TaskItemStatus.Completed && !task.CompletedAt.HasValue)
            {
                task.CompletedAt = DateTime.Now;
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
                task.CompletedAt = DateTime.Now;
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
    }
}