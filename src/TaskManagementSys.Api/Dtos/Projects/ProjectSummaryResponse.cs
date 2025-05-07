using System;
using TaskManagementSys.Core.Entities;

namespace TaskManagementSys.Api.Dtos.Projects
{
    public class ProjectSummaryResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public string StatusName => Status.ToString();
    }
} 