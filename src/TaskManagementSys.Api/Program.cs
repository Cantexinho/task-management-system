using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using DotNetEnv;
using TaskManagementSys.Infrastructure;
using TaskManagementSys.Infrastructure.Configuration;
using TaskManagementSys.Infrastructure.Data;

// TODO: Temp and not cool code 
var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"));
Console.WriteLine($"Looking for .env file at: {envPath}");
Env.Load(envPath);

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("TaskManagementSys.Api.Controllers", LogLevel.Debug);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Identity configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = EnvVars.GoogleClientId;
        options.ClientSecret = EnvVars.GoogleClientSecret;
    });

// Cookie policy
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.Name = "TaskManagementAuth";
    options.ExpireTimeSpan = TimeSpan.FromDays(EnvVars.JwtExpiryDays);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5010",
                "https://localhost:5010",
                "http://localhost:5011",
                "https://localhost:5011"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        var connectionString = EnvVars.ConnectionString;
        var dbPath = "";
        
        if (connectionString != null && connectionString.Contains("Data Source="))
        {
            int start = connectionString.IndexOf("Data Source=") + "Data Source=".Length;
            int end = connectionString.IndexOf(";", start);
            dbPath = end > start 
                ? connectionString.Substring(start, end - start)
                : connectionString.Substring(start);
                
            logger.LogInformation($"Database path: {dbPath}");
            
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                logger.LogInformation($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }
        }
        
        logger.LogInformation("Applying database migrations...");
        await context.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
        
        if (!string.IsNullOrEmpty(dbPath) && File.Exists(dbPath))
        {
            logger.LogInformation($"SQLite database file exists at: {dbPath}");
            logger.LogInformation($"File size: {new FileInfo(dbPath).Length} bytes");
        }
        else if (!string.IsNullOrEmpty(dbPath))
        {
            logger.LogWarning($"SQLite database file was not found at: {dbPath}");
        }
        
        logger.LogInformation("Seeding default roles and admin user...");
        await SeedIdentityDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorApp");

// Simple auth token middleware for development
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Auth-Token", out var authToken))
    {
        try
        {
            var decodedToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authToken.ToString()));
            var parts = decodedToken.Split(':');
            if (parts.Length == 2)
            {
                var userId = parts[0];
                var email = parts[1];
                
                // Create a simple claims identity
                var claims = new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, email)
                };
                
                var identity = new System.Security.Claims.ClaimsIdentity(claims, "TokenAuth");
                context.User = new System.Security.Claims.ClaimsPrincipal(identity);
            }
        }
        catch
        {
            // Invalid token, continue without authentication
        }
    }
    
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

async Task SeedIdentityDataAsync(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    string[] roleNames = { "Admin", "Manager", "User" };
    
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            logger.LogInformation($"Created role: {roleName}");
        }
    }
    
    var adminEmail = EnvVars.AdminEmail;
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        var newAdminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(newAdminUser, EnvVars.AdminPassword);
        
        if (result.Succeeded)
        {
            adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser != null)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation($"Created admin user: {adminEmail}");
            }
            else
            {
                logger.LogError("Admin user was created but could not be retrieved");
            }
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError($"Error creating admin user: {errors}");
        }
    }
}