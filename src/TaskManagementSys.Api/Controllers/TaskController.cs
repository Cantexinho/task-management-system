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
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public TasksController(
            ITaskService taskService, 
            ILogger<TasksController> logger,
            UserManager<IdentityUser> userManager)
        {
            _taskService = taskService;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
        {
            try
            {
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    var tasks = await _taskService.GetAllTasksAsync();
                    return Ok(tasks);
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null)
                    {
                        return Unauthorized("User ID not found");
                    }
                    
                    var tasks = await _taskService.GetTasksByUserIdAsync(userId);
                    return Ok(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTaskById(int id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }
                
                bool isAdmin = User.IsInRole("Admin");
                bool isManager = User.IsInRole("Manager");
                bool isAssignedToUser = task.Assignments?.Any(a => a.UserId == userId && a.IsActive) ?? false;
                bool isCreatedByUser = task.CreatedByUserId == userId;
                
                if (!(isAdmin || isManager || isAssignedToUser || isCreatedByUser))
                {
                    return Forbid();
                }
                
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving task with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // current user is the creator
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }
                
                task.CreatedByUserId = userId;
                task.CreatedAt = DateTime.UtcNow;
                
                var createdTask = await _taskService.CreateTaskAsync(task);
                return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            try
            {
                if (id != task.Id)
                {
                    return BadRequest("Task ID mismatch");
                }
                
                var existingTask = await _taskService.GetTaskByIdAsync(id);
                if (existingTask == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }
                
                bool isAdmin = User.IsInRole("Admin");
                bool isManager = User.IsInRole("Manager");
                bool isCreatedByUser = existingTask.CreatedByUserId == userId;
                bool isAssignedToUser = existingTask.Assignments?.Any(a => a.UserId == userId && a.IsActive) ?? false;
                
                if (!(isAdmin || isManager || isCreatedByUser))
                {
                    if (isAssignedToUser)
                    {
                        var updatedTask = existingTask;
                        updatedTask.Status = task.Status;
                        
                        if (task.Status == TaskItemStatus.Completed && !updatedTask.CompletedAt.HasValue)
                        {
                            updatedTask.CompletedAt = DateTime.UtcNow;
                        }
                        else if (task.Status != TaskItemStatus.Completed)
                        {
                            updatedTask.CompletedAt = null;
                        }
                        
                        await _taskService.UpdateTaskAsync(updatedTask);
                        return NoContent();
                    }
                    
                    return Forbid();
                }
                
                task.CreatedByUserId = existingTask.CreatedByUserId;
                task.CreatedAt = existingTask.CreatedAt;
                
                if (task.Status == TaskItemStatus.Completed && !existingTask.CompletedAt.HasValue)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else if (task.Status != TaskItemStatus.Completed)
                {
                    task.CompletedAt = null;
                }
                
                await _taskService.UpdateTaskAsync(task);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                // managers can only delete tasks they created
                if (User.IsInRole("Manager") && !User.IsInRole("Admin"))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null)
                    {
                        return Unauthorized("User ID not found");
                    }
                    
                    if (task.CreatedByUserId != userId)
                    {
                        return Forbid();
                    }
                }
                
                var result = await _taskService.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignTask(int id, [FromBody] TaskAssignmentRequest request)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                // check if user exists
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest("User not found");
                }
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }
                
                // new assignment
                var assignment = new TaskAssignment
                {
                    TaskItemId = id,
                    UserId = request.UserId,
                    AssignedById = userId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                await _taskService.AssignTaskAsync(assignment);
                return Ok(new { message = "Task assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning task with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/unassign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UnassignTask(int id, [FromBody] TaskAssignmentRequest request)
        {
            try
            {
                var result = await _taskService.UnassignTaskAsync(id, request.UserId);
                if (!result)
                {
                    return NotFound("Task assignment not found");
                }
                
                return Ok(new { message = "Task unassigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unassigning task with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class TaskAssignmentRequest
    {
        public required string UserId { get; set; }
    }
}