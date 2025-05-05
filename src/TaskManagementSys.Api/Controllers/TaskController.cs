using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Core.Services;

namespace TaskManagementSys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                return Ok(tasks);
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
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var result = await _taskService.DeleteTaskAsync(id);
                if (!result)
                {
                    return NotFound($"Task with ID {id} not found");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}