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
        public static int JwtExpiryDays => int.Parse(GetEnvVar("JWT_EXPIRY_DAYS", "14"));
        
        // Google Authentication
        public static string GoogleClientId => GetEnvVar("GOOGLE_CLIENT_ID", "");
        public static string GoogleClientSecret => GetEnvVar("GOOGLE_CLIENT_SECRET", "");
        
        // Admin User
        public static string AdminEmail => GetEnvVar("ADMIN_EMAIL", "");
        public static string AdminPassword => GetEnvVar("ADMIN_PASSWORD", "");

        // API Configuration
        public static int ApiPort => int.Parse(GetEnvVar("API_PORT", "5000"));
        public static string ApiHost => GetEnvVar("API_HOST", "localhost");
        public static int BlazorPort => int.Parse(GetEnvVar("BLAZOR_PORT", "5010"));
        public static string BlazorHost => GetEnvVar("BLAZOR_HOST", "localhost");

        // CORS Configuration
        public static string[] AllowedOrigins => GetEnvVar("ALLOWED_ORIGINS", "").Split(',', StringSplitOptions.RemoveEmptyEntries);

        // Identity Settings
        public static bool PasswordRequireDigit => bool.Parse(GetEnvVar("PASSWORD_REQUIRE_DIGIT", "true"));
        public static bool PasswordRequireLowercase => bool.Parse(GetEnvVar("PASSWORD_REQUIRE_LOWERCASE", "true"));
        public static bool PasswordRequireUppercase => bool.Parse(GetEnvVar("PASSWORD_REQUIRE_UPPERCASE", "true"));
        public static bool PasswordRequireNonAlphanumeric => bool.Parse(GetEnvVar("PASSWORD_REQUIRE_NON_ALPHANUMERIC", "true"));
        public static int PasswordMinLength => int.Parse(GetEnvVar("PASSWORD_MIN_LENGTH", "8"));
        public static int LockoutDurationMinutes => int.Parse(GetEnvVar("LOCKOUT_DURATION_MINUTES", "15"));
        public static int MaxFailedAttempts => int.Parse(GetEnvVar("MAX_FAILED_ATTEMPTS", "5"));
        public static string CookieName => GetEnvVar("COOKIE_NAME", "TaskManagementAuth");
        public static bool RequireUniqueEmail => bool.Parse(GetEnvVar("REQUIRE_UNIQUE_EMAIL", "true"));
        public static bool RequireConfirmedAccount => bool.Parse(GetEnvVar("REQUIRE_CONFIRMED_ACCOUNT", "false"));
        public static bool RequireConfirmedEmail => bool.Parse(GetEnvVar("REQUIRE_CONFIRMED_EMAIL", "false"));

        // API URLs
        public static string ApiBaseUrl => $"http://{ApiHost}:{ApiPort}";
        
        private static string GetEnvVar(string name, string defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
    }
} 