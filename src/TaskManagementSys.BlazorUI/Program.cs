using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskManagementSys.BlazorUI.Components;
using TaskManagementSys.BlazorUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ProjectService>();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
Console.WriteLine($"API Base URL configured as: {apiBaseUrl}");

// Configure HttpClient to include credentials and support cross-origin requests
builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    
    // Configure to include credentials (cookies) with requests
    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    
    return httpClient;
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();
