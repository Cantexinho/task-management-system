using System.Net.Http.Json;

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
                var response = await _httpClient.GetAsync("/api/Tasks/my-tasks");
                
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
} 