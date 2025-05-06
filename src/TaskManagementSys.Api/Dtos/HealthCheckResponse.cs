using System;
using System.Collections.Generic;

namespace TaskManagementSys.Api.Dtos
{
    public class HealthCheckResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Provider { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public string? DataSource { get; set; }
        public bool FileExists { get; set; }
        public long? FileSize { get; set; }
        public DateTime? LastModified { get; set; }
        public int TaskCount { get; set; }
        public Dictionary<string, List<TableColumnInfo>>? DatabaseSchema { get; set; }
    }
}