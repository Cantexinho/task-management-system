using System.Net.Http.Json;
using System.ComponentModel.DataAnnotations;
using TaskManagementSys.Core.Entities;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace TaskManagementSys.BlazorUI.Services
{
    public class TaskService
    {
        private readonly HttpClient _httpClient;

        public TaskService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TaskDto>> GetUserTasksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Tasks");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<TaskDto>>() ?? new List<TaskDto>();
                }
                
                return new List<TaskDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tasks: {ex.Message}");
                return new List<TaskDto>();
            }
        }

        public async Task<bool> UpdateTaskAsync(TaskDto task)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                
                var json = JsonSerializer.Serialize(task, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/api/Tasks/{task.Id}", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task: {ex.Message}");
                return false;
            }
        }
        
        public async Task<(TaskDto? Task, Dictionary<string, List<string>> ValidationErrors)> CreateTaskAsync(CreateTaskDto task)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                
                var json = JsonSerializer.Serialize(task, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"Sending task data: {json}");
                
                var response = await _httpClient.PostAsync("/api/Tasks", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
                    Console.WriteLine($"Task created with ID: {createdTask?.Id}");
                    return (createdTask, new Dictionary<string, List<string>>());
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error creating task: {response.StatusCode}, Response: {errorContent}");
                    
                    var validationErrors = new Dictionary<string, List<string>>();
                    
                    // Try to parse validation errors from response
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        try
                        {
                            // Parse model state errors from API response
                            using JsonDocument doc = JsonDocument.Parse(errorContent);
                            
                            if (doc.RootElement.TryGetProperty("errors", out JsonElement errors))
                            {
                                foreach (var property in errors.EnumerateObject())
                                {
                                    string fieldName = property.Name;
                                    var errorList = new List<string>();
                                    
                                    if (property.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var error in property.Value.EnumerateArray())
                                        {
                                            errorList.Add(error.GetString() ?? "Invalid value");
                                        }
                                    }
                                    else
                                    {
                                        errorList.Add(property.Value.GetString() ?? "Invalid value");
                                    }
                                    
                                    validationErrors[fieldName] = errorList;
                                }
                            }
                            else
                            {
                                // If not a structured error, add as a general error
                                validationErrors[""] = new List<string> { errorContent };
                            }
                        }
                        catch (JsonException)
                        {
                            // If JSON parsing fails, add the raw response as a general error
                            validationErrors[""] = new List<string> { errorContent };
                        }
                    }
                    else
                    {
                        // General error
                        validationErrors[""] = new List<string> { $"Server Error: {response.StatusCode}" };
                    }
                    
                    return (null, validationErrors);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception creating task: {ex.Message}");
                return (null, new Dictionary<string, List<string>> { { "", new List<string> { ex.Message } } });
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteTaskAsync(int taskId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Tasks/{taskId}");
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return (false, "You don't have permission to delete tasks. Only administrators and managers can delete tasks.");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task: {ex.Message}");
                return (false, ex.Message);
            }
        }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
    }
    
    public class CreateTaskDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public int? ProjectId { get; set; }
        
        [Required]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
        
        public List<int> CategoryIds { get; set; } = new List<int>();
        
        public List<string> AssigneeIds { get; set; } = new List<string>();
    }
} 