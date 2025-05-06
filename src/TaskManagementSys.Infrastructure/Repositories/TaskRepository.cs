using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagementSys.Core.Entities;
using TaskManagementSys.Infrastructure.Data;

namespace TaskManagementSys.Infrastructure.Repositories
{
    public class TaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Categories)
                .Include(t => t.Comments)
                .Include(t => t.Assignments)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Categories)
                .Include(t => t.Comments)
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem> UpdateTaskAsync(TaskItem task)
        {
            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty");
            }

            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Categories)
                .Include(t => t.Comments)
                .Include(t => t.Assignments)
                .Where(t => t.CreatedByUserId == userId || 
                           t.Assignments.Any(a => a.UserId == userId && a.IsActive))
                .ToListAsync();
        }

        public async Task<TaskAssignment> CreateTaskAssignmentAsync(TaskAssignment assignment)
        {
            _context.TaskAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> DeactivateTaskAssignmentsAsync(int taskId, string userId)
        {
            var assignments = await _context.TaskAssignments
                .Where(a => a.TaskItemId == taskId && a.UserId == userId && a.IsActive)
                .ToListAsync();

            if (assignments.Count == 0)
            {
                return false;
            }

            foreach (var assignment in assignments)
            {
                assignment.IsActive = false;
                assignment.DeactivatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskAssignment>> GetTaskAssignmentsByTaskIdAsync(int taskId)
        {
            return await _context.TaskAssignments
                .Where(a => a.TaskItemId == taskId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskAssignment>> GetActiveTaskAssignmentsByUserIdAsync(string userId)
        {
            return await _context.TaskAssignments
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();
        }
    }
}