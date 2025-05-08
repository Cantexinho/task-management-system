using System;

namespace TaskManagementSys.Infrastructure.Configuration
{
    public static class EnvVars
    {
        // Database
        public static string ConnectionString => GetEnvVar("CONNECTION_STRING", "");
        
        // Authentication
        public static string JwtSecret => GetEnvVar("JWT_SECRET", "");
        public static string JwtIssuer => GetEnvVar("JWT_ISSUER", "");
        public static string JwtAudience => GetEnvVar("JWT_AUDIENCE", "");
        public static int JwtExpiryDays => int.Parse(GetEnvVar("JWT_EXPIRY_DAYS", "0"));
        
        // Google Authentication
        public static string GoogleClientId => GetEnvVar("GOOGLE_CLIENT_ID", "");
        public static string GoogleClientSecret => GetEnvVar("GOOGLE_CLIENT_SECRET", "");
        
        // Admin User
        public static string AdminEmail => GetEnvVar("ADMIN_EMAIL", "");
        public static string AdminPassword => GetEnvVar("ADMIN_PASSWORD", "");
        
        private static string GetEnvVar(string name, string defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
} 