using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TaskManagementSys.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public class RegisterModel
        {
            public required string Email { get; set; }
            public required string Password { get; set; }
            public required string ConfirmPassword { get; set; }
        }

        public class LoginModel
        {
            public required string Email { get; set; }
            public required string Password { get; set; }
            public bool RememberMe { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state during registration: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var user = new IdentityUser { 
                UserName = model.Email, 
                Email = model.Email,
                EmailConfirmed = true // Automatically confirm email
            };
            
            _logger.LogInformation("Attempting to create user with email: {Email}", model.Email);
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password");
                
                // Add user to default role
                await _userManager.AddToRoleAsync(user, "User");
                
                // Auto sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Get roles and generate auth token
                var roles = await _userManager.GetRolesAsync(user);
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                    $"{user.Id}:{user.Email}:{string.Join(",", roles)}"
                ));
                
                return Ok(new { 
                    message = "User registered successfully",
                    authToken = authToken,
                    user = new {
                        email = user.Email,
                        userName = user.UserName,
                        roles = roles
                    }
                });
            }
            
            _logger.LogWarning("User registration failed: {Errors}", 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in");
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Generate a simple auth token (user ID and roles for development)
                    var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                        $"{user.Id}:{user.Email}:{string.Join(",", roles)}"
                    ));
                    
                    return Ok(new { 
                        message = "Login successful",
                        authToken = authToken, // Add auth token for cross-origin requests
                        user = new {
                            email = user.Email,
                            userName = user.UserName,
                            roles = roles
                        }
                    });
                }
                return BadRequest(new { message = "User not found" });
            }
            
            if (result.RequiresTwoFactor)
            {
                return BadRequest(new { message = "Two-factor authentication required" });
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out");
                return BadRequest(new { message = "Account locked out" });
            }
            
            return BadRequest(new { message = "Invalid login attempt" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("external-login")]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl = returnUrl ?? "/" });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl ?? "/");
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                return BadRequest(new { message = $"Error from external provider: {remoteError}" });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return BadRequest(new { message = "Error loading external login information" });
            }

            // sign in with the external provider
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider", info.LoginProvider);
                // Get the email to pass back to the client
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest(new { message = "Email not provided by external provider" });
                }
                
                var user = await _userManager.FindByEmailAsync(userEmail);
                
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                        $"{user.Id}:{user.Email}:{string.Join(",", roles)}"
                    ));
                    
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        var separator = returnUrl.Contains("?") ? "&" : "?";
                        returnUrl = $"{returnUrl}{separator}Email={Uri.EscapeDataString(userEmail)}&Token={Uri.EscapeDataString(authToken)}";
                    }
                }
                
                return Redirect(returnUrl ?? "/");
            }
            
            if (result.IsLockedOut)
            {
                return BadRequest(new { message = "Account locked out" });
            }
            
            // If no user account, create
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true // Automatically confirm email for external logins
                    };
                    
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "User");
                }
                
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                _logger.LogInformation("User created an account using {Name} provider", info.LoginProvider);
                
                // Generate auth token for new user
                var roles = await _userManager.GetRolesAsync(user);
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                    $"{user.Id}:{user.Email}:{string.Join(",", roles)}"
                ));
                
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    var separator = returnUrl.Contains("?") ? "&" : "?";
                    returnUrl = $"{returnUrl}{separator}Email={Uri.EscapeDataString(email)}&Token={Uri.EscapeDataString(authToken)}";
                }
                
                return Redirect(returnUrl ?? "/");
            }
            
            return BadRequest(new { message = "Error signing in with external provider" });
        }

        [Authorize]
        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            
            return Ok(new
            {
                email = user.Email,
                userName = user.UserName,
                roles = roles
            });
        }
    }
}