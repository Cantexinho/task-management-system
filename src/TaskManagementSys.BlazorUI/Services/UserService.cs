using System.Net.Http.Json;

namespace TaskManagementSys.BlazorUI.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UserDto>> GetAllUsers()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
            return response ?? new List<UserDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching users: {ex.Message}");
            return new List<UserDto>();
        }
    }

    public async Task<bool> UpdateUserRoles(string userId, List<string> roles)
    {
        try
        {
            var request = new UpdateUserRolesRequest { Roles = roles };
            var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/roles", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user roles: {ex.Message}");
            return false;
        }
    }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}

public class UpdateUserRolesRequest
{
    public List<string> Roles { get; set; } = new();
} 