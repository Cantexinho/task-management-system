namespace TaskManagementSys.Api.Dtos
{
    public class HealthCheckResponse
    {
        public string Status { get; set; } = default!;
        public string? Provider { get; set; }
        public string? ConnectionString { get; set; }
        public string? DataSource { get; set; }
        public bool FileExists { get; set; }
        public long? FileSize { get; set; }
        public DateTime? LastModified { get; set; }
        public int TaskCount { get; set; }
    }
}
