using TaskManagementSys.BlazorUI.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TaskManagementSys.BlazorUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HttpClient>(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7077") 
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
