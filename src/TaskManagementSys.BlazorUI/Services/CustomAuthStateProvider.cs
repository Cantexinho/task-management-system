using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace TaskManagementSys.BlazorUI.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var storedUserJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userSession");
                if (string.IsNullOrEmpty(storedUserJson))
                    return await Task.FromResult(new AuthenticationState(_anonymous));

                var userSession = JsonSerializer.Deserialize<UserSession>(storedUserJson);
                
                if (userSession == null)
                    return await Task.FromResult(new AuthenticationState(_anonymous));

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Email),
                    new Claim(ClaimTypes.Email, userSession.Email)
                }, "CustomAuth"));

                // Add roles if available
                if (userSession.Roles != null)
                {
                    foreach (var role in userSession.Roles)
                    {
                        ((ClaimsIdentity)claimsPrincipal.Identity!).AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }

                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task UpdateAuthenticationState(UserSession? userSession)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession != null)
            {
                var userSessionJson = JsonSerializer.Serialize(userSession);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userSession", userSessionJson);
                
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Email),
                    new Claim(ClaimTypes.Email, userSession.Email)
                }, "CustomAuth"));

                // Add roles if available
                if (userSession.Roles != null)
                {
                    foreach (var role in userSession.Roles)
                    {
                        ((ClaimsIdentity)claimsPrincipal.Identity!).AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userSession");
                claimsPrincipal = _anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }

    public class UserSession
    {
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
} 