using Microsoft.EntityFrameworkCore;
using System.IO;
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

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("http://localhost:5010")
            .AllowAnyHeader()
            .AllowAnyMethod();
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
        
        if (connectionString.Contains("Data Source="))
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
        
        logger.LogInformation("Ensuring database is created...");
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created successfully.");
        
        if (File.Exists(dbPath))
        {
            logger.LogInformation($"SQLite database file exists at: {dbPath}");
            logger.LogInformation($"File size: {new FileInfo(dbPath).Length} bytes");
        }
        else
        {
            logger.LogWarning($"SQLite database file was not found at: {dbPath}");
        }
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

app.UseAuthorization();

app.MapControllers();

app.Run();