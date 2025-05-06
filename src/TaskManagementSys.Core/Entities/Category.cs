using System.Collections.Generic;

namespace TaskManagementSys.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Color { get; set; }
        
        // Navigation properties
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}