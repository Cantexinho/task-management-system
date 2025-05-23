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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Task Management API", Version = "v1" });
    
    // Add security definition
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "X-Auth-Token",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add security requirement
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Identity configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
{
    // Password settings
    options.Password.RequireDigit = EnvVars.PasswordRequireDigit;
    options.Password.RequireLowercase = EnvVars.PasswordRequireLowercase;
    options.Password.RequireUppercase = EnvVars.PasswordRequireUppercase;
    options.Password.RequireNonAlphanumeric = EnvVars.PasswordRequireNonAlphanumeric;
    options.Password.RequiredLength = EnvVars.PasswordMinLength;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(EnvVars.LockoutDurationMinutes);
    options.Lockout.MaxFailedAccessAttempts = EnvVars.MaxFailedAttempts;
    
    // User settings
    options.User.RequireUniqueEmail = EnvVars.RequireUniqueEmail;
    options.SignIn.RequireConfirmedAccount = EnvVars.RequireConfirmedAccount;
    options.SignIn.RequireConfirmedEmail = EnvVars.RequireConfirmedEmail;
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
    options.Cookie.Name = EnvVars.CookieName;
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
        policy.WithOrigins(EnvVars.AllowedOrigins)
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
            if (parts.Length >= 3)  // Now expecting userId:email:roles
            {
                var userId = parts[0];
                var email = parts[1];
                var roles = parts[2].Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                // Create claims list with roles
                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, email)
                };
                
                // Add role claims
                foreach (var role in roles)
                {
                    claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                }
                
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