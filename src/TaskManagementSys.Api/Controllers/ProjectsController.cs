using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Core.Services;

namespace TaskManagementSys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectsController(
            IProjectService projectService,
            ILogger<ProjectsController> logger,
            UserManager<IdentityUser> userManager)
        {
            _projectService = projectService;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
        {
            try
            {
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    var projects = await _projectService.GetAllProjectsAsync();
                    return Ok(projects);
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null)
                    {
                        return Unauthorized("User ID not found");
                    }

                    var projects = await _projectService.GetProjectsByUserIdAsync(userId);
                    return Ok(projects);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProjectById(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);

                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }

                bool isAdmin = User.IsInRole("Admin");
                bool isManager = User.IsInRole("Manager");
                bool isCreatedByUser = project.CreatedByUserId == userId;

                if (!(isAdmin || isManager || isCreatedByUser))
                {
                    return Forbid();
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving project with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(Project project)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }

                project.CreatedByUserId = userId;

                var createdProject = await _projectService.CreateProjectAsync(project);
                return CreatedAtAction(nameof(GetProjectById), new { id = createdProject.Id }, createdProject);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, Project project)
        {
            try
            {
                if (id != project.Id)
                {
                    return BadRequest("Project ID mismatch");
                }

                var existingProject = await _projectService.GetProjectByIdAsync(id);
                if (existingProject == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }

                bool isAdmin = User.IsInRole("Admin");
                bool isManager = User.IsInRole("Manager");
                bool isCreatedByUser = existingProject.CreatedByUserId == userId;

                if (!(isAdmin || isManager || isCreatedByUser))
                {
                    return Forbid();
                }

                await _projectService.UpdateProjectAsync(project);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                var result = await _projectService.DeleteProjectAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return StatusCode(500, "Failed to delete project");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting project with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetProjectTasks(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }

                bool isAdmin = User.IsInRole("Admin");
                bool isManager = User.IsInRole("Manager");
                bool isCreatedByUser = project.CreatedByUserId == userId;

                if (!(isAdmin || isManager || isCreatedByUser))
                {
                    return Forbid();
                }

                var tasks = await _projectService.GetTasksByProjectIdAsync(id);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for project with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 