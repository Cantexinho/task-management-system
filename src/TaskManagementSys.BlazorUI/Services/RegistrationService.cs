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
            else if (password.Length < 8)
            {
                return "Password must be at least 8 characters long";
            }
            else if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return "Password must contain at least one uppercase letter";
            }
            else if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return "Password must contain at least one lowercase letter";
            }
            else if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return "Password must contain at least one number";
            }
            else if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                return "Password must contain at least one special character";
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

        public (int strength, string text, string cssClass) CheckPasswordStrength(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (0, "", "");
            }

            int score = 0;

            if (password.Length > 8)
                score += 20;
            else if (password.Length > 6)
                score += 10;

            if (Regex.IsMatch(password, "[a-z]"))
                score += 15;

            if (Regex.IsMatch(password, "[A-Z]"))
                score += 20;

            if (Regex.IsMatch(password, "[0-9]"))
                score += 20;

            if (Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                score += 25;

            int strength = Math.Min(100, score);
            string strengthText;
            string barClass;

            if (strength < 40)
            {
                strengthText = "Weak";
                barClass = "progress-bar bg-danger";
            }
            else if (strength < 70)
            {
                strengthText = "Medium";
                barClass = "progress-bar bg-warning";
            }
            else
            {
                strengthText = "Strong";
                barClass = "progress-bar bg-success";
            }

            return (strength, strengthText, barClass);
        }

        public bool ValidateForm(string? email, string? password, string? confirmPassword, out string emailError, out string passwordError, out string confirmPasswordError)
        {
            emailError = ValidateEmail(email);
            passwordError = ValidatePassword(password);
            confirmPasswordError = ValidateConfirmPassword(password, confirmPassword);

            bool hasValidationErrors = !string.IsNullOrEmpty(emailError) ||
                                     !string.IsNullOrEmpty(passwordError) ||
                                     !string.IsNullOrEmpty(confirmPasswordError);

            return !hasValidationErrors;
        }

        public async Task<bool> RegisterAsync(string? email, string? password, string? confirmPassword)
        {
            return await _authService.RegisterAsync(email ?? string.Empty, password ?? string.Empty, confirmPassword ?? string.Empty);
        }

        public string GetErrorMessage(string? email, string? password, string? confirmPassword, string emailError, string passwordError, string confirmPasswordError)
        {
            email ??= string.Empty;
            password ??= string.Empty;
            confirmPassword ??= string.Empty;

            if (string.IsNullOrEmpty(email))
            {
                return "Email is required.";
            }

            if (!string.IsNullOrEmpty(emailError))
            {
                return emailError;
            }

            if (string.IsNullOrEmpty(password))
            {
                return "Password is required.";
            }

            if (!string.IsNullOrEmpty(passwordError))
            {
                return passwordError;
            }

            if (string.IsNullOrEmpty(confirmPassword))
            {
                return "Please confirm your password.";
            }

            if (!string.IsNullOrEmpty(confirmPasswordError))
            {
                return confirmPasswordError;
            }

            return "Please fill in all required fields correctly.";
        }
    }
} 