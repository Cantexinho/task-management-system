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

        public async Task<(UserInfo? userInfo, string? authToken)> LoginAsync(string email, string password)
        {
            var loginData = new
            {
                Email = email,
                Password = password,
                RememberMe = false
            };

            Console.WriteLine($"Sending login request to: {_httpClient.BaseAddress}/api/Account/login");
            var response = await _httpClient.PostAsJsonAsync("/api/Account/login", loginData);
            
            Console.WriteLine($"Login response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                Console.WriteLine($"Login response received: {loginResponse?.Message}");
                
                if (loginResponse?.User != null)
                {
                    var userInfo = new UserInfo
                    {
                        Email = loginResponse.User.Email,
                        UserName = loginResponse.User.UserName,
                        Roles = loginResponse.User.Roles ?? new List<string>()
                    };
                    
                    Console.WriteLine($"User info extracted: Email={userInfo.Email}");
                    return (userInfo, loginResponse.AuthToken);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed with status {response.StatusCode}: {errorContent}");
            }
            
            return (null, null);
        }

        public async Task<(UserInfo? userInfo, string? authToken)> RegisterAsync(string email, string password, string confirmPassword)
        {
            var registerData = new
            {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Account/register", registerData);
            
            if (response.IsSuccessStatusCode)
            {
                var registerResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                
                if (registerResponse?.User != null)
                {
                    var userInfo = new UserInfo
                    {
                        Email = registerResponse.User.Email,
                        UserName = registerResponse.User.UserName,
                        Roles = registerResponse.User.Roles ?? new List<string>()
                    };
                    
                    return (userInfo, registerResponse.AuthToken);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Registration failed. Status: {response.StatusCode}, Error: {errorContent}");
            }
            
            return (null, null);
        }

        public async Task LogoutAsync()
        {
            await _httpClient.PostAsync("/api/Account/logout", null);
        }
        
        public async Task<UserInfo?> GetUserInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Account/user-info");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserInfo>();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user info: {ex.Message}");
                return null;
            }
        }

        public void SetAuthToken(string? token)
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Auth-Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", token);
            }
        }
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public UserData User { get; set; } = new UserData();
    }

    public class UserData
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
} 