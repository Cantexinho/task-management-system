namespace TaskManagementSys.Api.Dtos
{
    public class TableColumnInfo
    {
        public int ColumnId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool NotNull { get; set; }
        public string? DefaultValue { get; set; }
        public bool PrimaryKey { get; set; }
    }
}