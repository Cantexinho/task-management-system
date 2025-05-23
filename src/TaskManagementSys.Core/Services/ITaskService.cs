using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Core.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync();
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> CreateTaskAsync(TaskItem task);
        Task<TaskItem> UpdateTaskAsync(TaskItem task);
        Task<bool> DeleteTaskAsync(int id);
        
        Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(string userId);
        
        Task<TaskAssignment> AssignTaskAsync(TaskAssignment assignment);
        Task<bool> UnassignTaskAsync(int taskId, string userId);
    }
}