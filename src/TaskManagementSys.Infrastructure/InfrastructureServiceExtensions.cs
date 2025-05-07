using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSys.Core.Services;
using TaskManagementSys.Infrastructure.Data;
using TaskManagementSys.Infrastructure.Repositories;
using TaskManagementSys.Infrastructure.Services;

namespace TaskManagementSys.Infrastructure
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("TaskManagementSys.Infrastructure")));

            services.AddScoped<TaskRepository>();
            services.AddScoped<ProjectRepository>();
            
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IProjectService, ProjectService>();
            
            return services;
        }
    }
}