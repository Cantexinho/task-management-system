using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Identity table customizations
            modelBuilder.Entity<IdentityUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                entity.HasIndex(e => e.CreatedByUserId);
                entity.HasIndex(e => e.Status);
                entity.ToTable(t => t.HasCheckConstraint("CK_Project_EndDate_After_StartDate", 
                    "EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate"));
            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(t => t.Project)
                      .WithMany(p => p.Tasks)
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Categories)
                      .WithMany(c => c.Tasks)
                      .UsingEntity(j => j.ToTable("TaskItemCategory"));
                
                entity.HasIndex(e => e.CreatedByUserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DueDate);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => new { e.Status, e.DueDate });
                
                entity.ToTable(t => t.HasCheckConstraint("CK_TaskItem_CompletedAt", 
                    "Status != 3 OR CompletedAt IS NOT NULL"));
                
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);

                entity.HasOne(c => c.TaskItem)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(c => c.TaskItemId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.ToTable(t => t.HasCheckConstraint("CK_Comment_Content_NotEmpty", 
                    "LENGTH(TRIM(Content)) > 0"));
            });

            modelBuilder.Entity<TaskAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(ta => ta.TaskItem)
                      .WithMany(t => t.Assignments)
                      .HasForeignKey(ta => ta.TaskItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ta => new { ta.TaskItemId, ta.UserId })
                      .IsUnique()
                      .HasFilter("IsActive = 1");
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.AssignedById);
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.ToTable(t => t.HasCheckConstraint("CK_TaskAssignment_DeactivatedAt", 
                    "IsActive = 1 OR DeactivatedAt IS NOT NULL"));
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50);
                
                entity.HasIndex(e => e.Name);
                entity.ToTable(t => t.HasCheckConstraint("CK_Category_Color_Format", 
                    "Color IS NULL OR Color LIKE '#%' AND LENGTH(Color) <= 9"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Category_Name_NotEmpty", 
                    "LENGTH(TRIM(Name)) > 0"));
            });

            modelBuilder.Entity<Project>().HasData(
                new Project
                {
                    Id = 1,
                    Name = "Website Redesign",
                    Description = "Complete overhaul of the company website",
                    StartDate = new DateTime(2024, 4, 1),
                    Status = ProjectStatus.Active,
                    CreatedByUserId = "admin-user"
                }
            );

            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem
                {
                    Id = 1,
                    Title = "Initial project setup",
                    Description = "Set up the project structure",
                    DueDate = new DateTime(2024, 5, 12),
                    Priority = TaskPriority.High,
                    Status = TaskItemStatus.Completed,
                    CreatedAt = new DateTime(2024, 5, 3),
                    CompletedAt = new DateTime(2024, 5, 5),
                    ProjectId = 1,
                    CreatedByUserId = "admin-user"
                }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Bug", Color = "#FF0000" },
                new Category { Id = 2, Name = "Feature", Color = "#00FF00" },
                new Category { Id = 3, Name = "Documentation", Color = "#0000FF" }
            );
        }

        public async Task MigrateAsync()
        {
            await Database.MigrateAsync();
        }
    }
}