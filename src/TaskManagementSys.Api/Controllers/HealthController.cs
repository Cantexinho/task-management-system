using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Infrastructure.Data;
using TaskManagementSys.Api.Dtos;

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
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = "system"
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

                // Get database schema information
                var schemaInfo = new Dictionary<string, List<TableColumnInfo>>();
                
                if (canConnect)
                {
                    var tables = GetAllTables();
                    
                    foreach (var table in tables)
                    {
                        var columns = GetTableColumns(table);
                        schemaInfo[table] = columns;
                    }
                }

                return Ok(new HealthCheckResponse
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    Provider = _context.Database.ProviderName,
                    ConnectionString = connectionString,
                    DataSource = dataSource,
                    FileExists = fileExists,
                    FileSize = fileSize,
                    LastModified = lastModified,
                    TaskCount = taskCount,
                    DatabaseSchema = schemaInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("schema")]
        public IActionResult GetDatabaseSchema()
        {
            try
            {
                var schemaInfo = new Dictionary<string, List<TableColumnInfo>>();
                var tables = GetAllTables();
                
                foreach (var table in tables)
                {
                    var columns = GetTableColumns(table);
                    schemaInfo[table] = columns;
                }

                return Ok(schemaInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve database schema");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        private List<string> GetAllTables()
        {
            // SQLite specific query to get all tables
            var tables = new List<string>();
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'ef_%';";
                
                _context.Database.OpenConnection();
                
                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        tables.Add(result.GetString(0));
                    }
                }
            }
            
            return tables;
        }

        private List<TableColumnInfo> GetTableColumns(string tableName)
        {
            var columns = new List<TableColumnInfo>();
            
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"PRAGMA table_info('{tableName}');";
                
                if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    _context.Database.OpenConnection();
                }
                
                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        var columnInfo = new TableColumnInfo
                        {
                            ColumnId = result.GetInt32(0),
                            Name = result.GetString(1),
                            Type = result.GetString(2),
                            NotNull = result.GetBoolean(3),
                            DefaultValue = result.IsDBNull(4) ? null : result.GetString(4),
                            PrimaryKey = result.GetInt32(5) > 0
                        };
                        
                        columns.Add(columnInfo);
                    }
                }
            }
            
            return columns;
        }
    }
}
