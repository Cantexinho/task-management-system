using System.Net.Http.Json;

namespace TaskManagementSys.BlazorUI.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProjectDto>> GetUserProjectsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Projects/my-projects");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ProjectDto>>() ?? new List<ProjectDto>();
                }
                
                return new List<ProjectDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching projects: {ex.Message}");
                return new List<ProjectDto>();
            }
        }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public double CompletionPercentage { get; set; }
    }
} 