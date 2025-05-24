using System.Text.RegularExpressions;

namespace TaskManagementSys.BlazorUI.Services
{
    public class RegistrationService
    {
        private readonly AuthService _authService;

        public RegistrationService(AuthService authService)
        {
            _authService = authService;
        }

        public string ValidateEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return "Email is required";
            }
            else if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return "Please enter a valid email address";
            }

            return string.Empty;
        }

        public string ValidatePassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "Password is required";
            }

            return string.Empty;
        }

        public string ValidateConfirmPassword(string? password, string? confirmPassword)
        {
            if (string.IsNullOrEmpty(confirmPassword))
            {
                return "Confirm Password is required";
            }
            else if (password != confirmPassword)
            {
                return "Passwords do not match";
            }

            return string.Empty;
        }

        public async Task<(UserInfo? userInfo, string? authToken)> RegisterAsync(string? email, string? password, string? confirmPassword)
        {
            return await _authService.RegisterAsync(email ?? string.Empty, password ?? string.Empty, confirmPassword ?? string.Empty);
        }
    }
} 