using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using TaskManagementSys.Api.Dtos.Projects;
using TaskManagementSys.Api.Dtos.Tasks;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Api.Mapping
{
    public static class DtoMappers
    {
        // Task mappers
        public static TaskItem ToEntity(this CreateTaskRequest dto, string userId)
        {
            return new TaskItem
            {
                Title = dto.Title ?? string.Empty,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Status = dto.Status,
                ProjectId = dto.ProjectId,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
        }
        
        public static void UpdateEntity(this UpdateTaskRequest dto, TaskItem entity)
        {
            entity.Title = dto.Title ?? string.Empty;
            entity.Description = dto.Description;
            entity.DueDate = dto.DueDate;
            entity.Priority = dto.Priority;
            entity.Status = dto.Status;
            entity.ProjectId = dto.ProjectId;
        }
        
        public static TaskResponse ToResponse(this TaskItem entity, IdentityUser? creator = null)
        {
            return new TaskResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                DueDate = entity.DueDate,
                Priority = entity.Priority,
                Status = entity.Status,
                CreatedAt = entity.CreatedAt,
                CompletedAt = entity.CompletedAt,
                ProjectId = entity.ProjectId,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUserName = creator?.UserName,
                Project = entity.Project?.ToSummaryResponse(),
                Categories = entity.Categories?.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color
                }).ToList() ?? new List<CategoryResponse>(),
                Assignments = entity.Assignments?.Select(a => new TaskAssignmentResponse
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    AssignedAt = a.AssignedAt,
                    IsActive = a.IsActive
                }).ToList() ?? new List<TaskAssignmentResponse>()
            };
        }
        
        // Project mappers
        public static Project ToEntity(this CreateProjectRequest dto, string userId)
        {
            return new Project
            {
                Name = dto.Name ?? string.Empty,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                CreatedByUserId = userId
            };
        }
        
        public static void UpdateEntity(this UpdateProjectRequest dto, Project entity)
        {
            entity.Name = dto.Name ?? string.Empty;
            entity.Description = dto.Description;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Status = dto.Status;
        }
        
        public static ProjectResponse ToResponse(this Project entity, IdentityUser? creator = null)
        {
            return new ProjectResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUserName = creator?.UserName,
                Tasks = entity.Tasks?.Select(t => new TaskSummaryResponse
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status
                }).ToList() ?? new List<TaskSummaryResponse>()
            };
        }
        
        public static ProjectSummaryResponse ToSummaryResponse(this Project entity)
        {
            return new ProjectSummaryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Status = entity.Status
            };
        }
    }
} 