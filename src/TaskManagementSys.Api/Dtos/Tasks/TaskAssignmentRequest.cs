using System.ComponentModel.DataAnnotations;

namespace TaskManagementSys.Api.Dtos.Tasks
{
    public class TaskAssignmentRequest
    {
        [Required]
        public string? UserId { get; set; }
    }
} 