using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TaskManagementSys.Infrastructure;
using TaskManagementSys.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

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
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Google auth settings
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });

// Cookie policy
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("http://localhost:5010")
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
        
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
    
    var adminEmail = "admin@taskmanagement.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        var newAdminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(newAdminUser, "Admin123!");
        
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