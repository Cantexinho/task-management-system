using System;
using System.ComponentModel.DataAnnotations;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Api.Dtos.Projects
{
    public class CreateProjectRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string? Name { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        [Required]
        [EnumDataType(typeof(ProjectStatus))]
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    }
} 