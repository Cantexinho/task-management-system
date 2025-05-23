using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;

namespace TaskManagementSys.BlazorUI.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // First, restore auth token from localStorage if it exists
                var storedAuthToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                if (!string.IsNullOrEmpty(storedAuthToken))
                {
                    _httpClient.DefaultRequestHeaders.Remove("X-Auth-Token");
                    _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", storedAuthToken);
                }

                // Try to check if user is authenticated on the server
                var userInfo = await GetUserInfoFromServerAsync();
                
                if (userInfo != null && !string.IsNullOrEmpty(userInfo.Email))
                {
                    // User is authenticated on server, update local state
                    var userSession = new UserSession
                    {
                        Email = userInfo.Email,
                        Roles = userInfo.Roles ?? new List<string>()
                    };
                    
                    // Update localStorage
                    var userSessionJson = JsonSerializer.Serialize(userSession);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userSession", userSessionJson);
                    
                    return CreateAuthenticatedState(userSession);
                }
                
                // Fallback to localStorage for client-side state
                var storedUserJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userSession");
                if (!string.IsNullOrEmpty(storedUserJson))
                {
                    var userSession = JsonSerializer.Deserialize<UserSession>(storedUserJson);
                    if (userSession != null)
                    {
                        return CreateAuthenticatedState(userSession);
                    }
                }
                
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
            catch
            {
                // Clear any stale state on error
                try
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userSession");
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                    _httpClient.DefaultRequestHeaders.Remove("X-Auth-Token");
                }
                catch { }
                
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        private async Task<UserInfo?> GetUserInfoFromServerAsync()
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
            catch
            {
                return null;
            }
        }

        private AuthenticationState CreateAuthenticatedState(UserSession userSession)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userSession.Email),
                new Claim(ClaimTypes.Email, userSession.Email),
                new Claim(ClaimTypes.NameIdentifier, userSession.UserId ?? userSession.Email)
            };

            // Add roles if available
            if (userSession.Roles != null)
            {
                foreach (var role in userSession.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "ServerAuth"));
            return new AuthenticationState(claimsPrincipal);
        }

        public async Task UpdateAuthenticationState(UserSession? userSession, string? authToken = null)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession != null)
            {
                var userSessionJson = JsonSerializer.Serialize(userSession);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userSession", userSessionJson);
                
                // Store auth token separately
                if (!string.IsNullOrEmpty(authToken))
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", authToken);
                    // Set the auth token on the HttpClient
                    _httpClient.DefaultRequestHeaders.Remove("X-Auth-Token");
                    _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", authToken);
                }
                
                claimsPrincipal = CreateAuthenticatedState(userSession).User;
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userSession");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                _httpClient.DefaultRequestHeaders.Remove("X-Auth-Token");
                claimsPrincipal = _anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
        
        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("/api/Account/logout", null);
            }
            catch { }
            
            await UpdateAuthenticationState(null);
        }
    }

    public class UserSession
    {
        public string Email { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
} 