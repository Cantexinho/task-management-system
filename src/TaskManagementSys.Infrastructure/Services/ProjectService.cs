using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Core.Services;
using TaskManagementSys.Infrastructure.Repositories;

namespace TaskManagementSys.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ProjectRepository _projectRepository;

        public ProjectService(ProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _projectRepository.GetAllProjectsAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _projectRepository.GetProjectByIdAsync(id);
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            if (string.IsNullOrEmpty(project.Name))
            {
                throw new ArgumentException("Project name cannot be null or empty");
            }
            
            if (string.IsNullOrEmpty(project.CreatedByUserId))
            {
                throw new ArgumentException("Creator ID cannot be null or empty");
            }
            
            // Set default values if not provided
            if (project.Status == 0)
            {
                project.Status = ProjectStatus.Planning;
            }
            
            return await _projectRepository.CreateProjectAsync(project);
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var existingProject = await _projectRepository.GetProjectByIdAsync(project.Id);
            if (existingProject == null)
            {
                throw new ArgumentException($"Project with ID {project.Id} not found");
            }
            
            if (string.IsNullOrEmpty(project.Name))
            {
                throw new ArgumentException("Project name cannot be null or empty");
            }
            
            // Keep original creator
            project.CreatedByUserId = existingProject.CreatedByUserId;
            
            return await _projectRepository.UpdateProjectAsync(project);
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            return await _projectRepository.DeleteProjectAsync(id);
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }
            
            return await _projectRepository.GetProjectsByUserIdAsync(userId);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _projectRepository.GetTasksByProjectIdAsync(projectId);
        }
    }
} 