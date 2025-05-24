using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagementSys.Api.Dtos.Users;

namespace TaskManagementSys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }

                var users = await _userManager.Users.ToListAsync();
                var responses = new List<UserResponse>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    responses.Add(new UserResponse
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        UserName = user.UserName ?? string.Empty,
                        Roles = roles.ToList(),
                        EmailConfirmed = user.EmailConfirmed,
                        LockoutEnabled = user.LockoutEnabled,
                        LockoutEnd = user.LockoutEnd
                    });
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponse>> GetUserById(string id)
        {
            try
            {
                var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (requestingUserId == null)
                {
                    return Unauthorized("User ID not found");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (requestingUserId == null)
                {
                    return Unauthorized("User ID not found");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                // Validate roles
                var validRoles = new[] { "User", "Manager", "Admin" };
                if (request.Roles.Any(r => !validRoles.Contains(r)))
                {
                    return BadRequest("Invalid role specified. Valid roles are: User, Manager, Admin");
                }

                // Prevent self-role modification
                if (requestingUserId == id)
                {
                    return BadRequest("You cannot modify your own roles");
                }

                // Remove existing roles
                var existingRoles = await _userManager.GetRolesAsync(user);
                if (existingRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove existing roles: {Errors}", 
                            string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        return StatusCode(500, "Failed to update user roles");
                    }
                }

                // Add new roles
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add new roles: {Errors}", 
                        string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return StatusCode(500, "Failed to update user roles");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating roles for user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("current")]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("User ID not found");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Current user not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 