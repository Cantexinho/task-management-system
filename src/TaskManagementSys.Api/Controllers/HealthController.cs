using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Infrastructure.Data;

namespace TaskManagementSys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<HealthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("db-status")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                bool canConnect = await _context.Database.CanConnectAsync();
                int taskCount = await _context.Tasks.CountAsync();

                if (taskCount == 0 && canConnect)
                {
                    var testTask = new TaskItem
                    {
                        Title = "Test Task",
                        Description = "Auto-generated for health check",
                        DueDate = DateTime.Now.AddDays(1),
                        Priority = TaskPriority.Medium,
                        Status = TaskItemStatus.Todo,
                        CreatedAt = DateTime.Now
                    };

                    _context.Tasks.Add(testTask);
                    await _context.SaveChangesAsync();
                    taskCount = 1;
                    _logger.LogInformation("Created a test task.");
                }

                string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Not configured";
                string? dataSource = connectionString
                    .Split(';')
                    .FirstOrDefault(s => s.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                    ?.Substring("Data Source=".Length);

                bool fileExists = !string.IsNullOrEmpty(dataSource) && System.IO.File.Exists(dataSource);
                long? fileSize = fileExists ? new FileInfo(dataSource!).Length : null;
                DateTime? lastModified = fileExists ? new FileInfo(dataSource!).LastWriteTime : null;

                return Ok(new
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    Provider = _context.Database.ProviderName,
                    File = new
                    {
                        Path = dataSource,
                        Exists = fileExists,
                        SizeBytes = fileSize,
                        LastModified = lastModified
                    },
                    TaskCount = taskCount,
                    Environment = new
                    {
                        Machine = Environment.MachineName,
                        OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
