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
                return BadRequest(ModelState);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password");
                
                // user to default role
                await _userManager.AddToRoleAsync(user, "User");
                
                // auto sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return Ok(new { message = "User registered successfully" });
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return BadRequest(ModelState);
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
                    
                    return Ok(new { 
                        message = "Login successful",
                        user = new {
                            email = user.Email,
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