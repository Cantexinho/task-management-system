using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace TaskManagementSys.BlazorUI.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        
        public string BaseUrl => _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var loginData = new
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Account/login", loginData);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return true;
            }
            
            return false;
        }

        public async Task<bool> RegisterAsync(string email, string password, string confirmPassword)
        {
            var registerData = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Account/register", registerData);
            
            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync()
        {
            await _httpClient.PostAsync("/api/Account/logout", null);
        }
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
} 