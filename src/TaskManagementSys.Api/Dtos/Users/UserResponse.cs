namespace TaskManagementSys.Api.Dtos.Users
{
    public class UserResponse
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required List<string> Roles { get; set; }
        public required bool EmailConfirmed { get; set; }
        public required bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }
} 