using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Core.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
        
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId);
        Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId);
    }
} 