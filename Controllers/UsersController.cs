using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Application.Dtos.Users;
using PosBackend.Application.Interfaces;
using PosBackend.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            PosDbContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            ILogger<UsersController> logger)
        {
            _context = context;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ToListAsync();

                var userDtos = new List<UserDetailDto>();
                foreach (var user in users)
                {
                    var roles = await _roleRepository.GetUserRolesAsync(user.UserId);
                    var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.UserId);

                    _logger.LogInformation($"Mapping user {user.UserName} (ID: {user.UserId}) with LastLogin: {user.LastLogin}");

                    _logger.LogInformation($"User {user.UserName} has LastLogin value: {user.LastLogin}");

                    var userDto = new UserDetailDto
                    {
                        Id = user.UserId,
                        Username = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLogin = user.LastLogin,
                        Roles = roles.Select(r => r.Name ?? string.Empty).ToList(),
                        Permissions = permissions.Select(p => p.Code).ToList()
                    };

                    _logger.LogInformation($"Mapped DTO for {userDto.Username} with LastLogin: {userDto.LastLogin}, LastLoginString: {userDto.LastLoginString}, LastLoginFormatted: {userDto.LastLoginFormatted}");
                    userDtos.Add(userDto);
                }

                // Log the serialized data for debugging
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };
                var serializedData = JsonSerializer.Serialize(userDtos, options);
                _logger.LogInformation($"Serialized user data: {serializedData}");

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { error = "Failed to retrieve users", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _roleRepository.GetUserRolesAsync(user.UserId);
                var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(user.UserId);

                _logger.LogInformation($"GetUser: Found user {user.UserName} (ID: {user.UserId}) with LastLogin: {user.LastLogin}");

                _logger.LogInformation($"GetUser: User {user.UserName} has LastLogin value: {user.LastLogin}");

                var userDto = new UserDetailDto
                {
                    Id = user.UserId,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin,
                    Roles = roles.Select(r => r.Name ?? string.Empty).ToList(),
                    Permissions = permissions.Select(p => p.Code).ToList()
                };

                _logger.LogInformation($"GetUser: Mapped DTO for {userDto.Username} with LastLogin: {userDto.LastLogin}, LastLoginString: {userDto.LastLoginString}, LastLoginFormatted: {userDto.LastLoginFormatted}");

                // Log the serialized data for debugging
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };
                var serializedData = JsonSerializer.Serialize(userDto, options);
                _logger.LogInformation($"Serialized single user data: {serializedData}");

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new { error = $"Failed to retrieve user {id}", message = ex.Message });
            }
        }

        [HttpPost]
        // Temporarily allowing all authenticated users to create users for testing
        [Authorize]
        public async Task<ActionResult<UserDetailDto>> CreateUser(UserDetailDto userDto)
        {
            try
            {
                // Check if user with same email already exists
                var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
                if (existingUser != null)
                {
                    return Conflict(new { error = $"User with email '{userDto.Email}' already exists" });
                }

                // Create new user
                var user = new User
                {
                    UserName = userDto.Username,
                    Email = userDto.Email,
                    IsActive = userDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateUserAsync(user);

                // Assign roles if any
                if (userDto.Roles != null && userDto.Roles.Any())
                {
                    foreach (var roleName in userDto.Roles)
                    {
                        var role = await _roleRepository.GetByNameAsync(roleName);
                        if (role != null)
                        {
                            await _roleRepository.AssignRoleToUserAsync(role.Id, createdUser.UserId);
                        }
                    }
                }

                // Return the created user with roles and permissions
                return await GetUser(createdUser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { error = "Failed to create user", message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateUser(int id, UserDetailDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest(new { error = "User ID mismatch" });
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Check if updating email to one that already exists
                if (user.Email != userDto.Email)
                {
                    var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
                    if (existingUser != null && existingUser.UserId != id)
                    {
                        return Conflict(new { error = $"User with email '{userDto.Email}' already exists" });
                    }
                }

                // Update user properties
                user.UserName = userDto.Username;
                user.Email = userDto.Email;
                user.IsActive = userDto.IsActive;

                var result = await _userRepository.UpdateUserAsync(user);
                if (!result)
                {
                    return NotFound();
                }

                // Update roles if provided
                if (userDto.Roles != null)
                {
                    // Get current roles
                    var currentRoles = await _roleRepository.GetUserRolesAsync(id);
                    var currentRoleNames = currentRoles.Select(r => r.Name ?? string.Empty).ToList();

                    // Roles to add
                    var rolesToAdd = userDto.Roles.Except(currentRoleNames).ToList();

                    // Roles to remove
                    var rolesToRemove = currentRoleNames.Except(userDto.Roles).ToList();

                    // Add new roles
                    foreach (var roleName in rolesToAdd)
                    {
                        var role = await _roleRepository.GetByNameAsync(roleName);
                        if (role != null)
                        {
                            await _roleRepository.AssignRoleToUserAsync(role.Id, id);
                        }
                    }

                    // Remove roles
                    foreach (var roleName in rolesToRemove)
                    {
                        var role = await _roleRepository.GetByNameAsync(roleName);
                        if (role != null)
                        {
                            await _roleRepository.RemoveRoleFromUserAsync(role.Id, id);
                        }
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { error = $"Failed to update user {id}", message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userRepository.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { error = $"Failed to delete user {id}", message = ex.Message });
            }
        }

        // POST: api/Users/{id}/roles
        [HttpPost("{id}/roles")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignRolesToUser(int id, [FromBody] List<string> roleNames)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Get current roles
                var currentRoles = await _roleRepository.GetUserRolesAsync(id);
                var currentRoleNames = currentRoles.Select(r => r.Name ?? string.Empty).ToList();

                // Roles to add
                var rolesToAdd = roleNames.Except(currentRoleNames).ToList();

                // Roles to remove
                var rolesToRemove = currentRoleNames.Except(roleNames).ToList();

                // Add new roles
                foreach (var roleName in rolesToAdd)
                {
                    var role = await _roleRepository.GetByNameAsync(roleName);
                    if (role != null)
                    {
                        await _roleRepository.AssignRoleToUserAsync(role.Id, id);
                    }
                }

                // Remove roles
                foreach (var roleName in rolesToRemove)
                {
                    var role = await _roleRepository.GetByNameAsync(roleName);
                    if (role != null)
                    {
                        await _roleRepository.RemoveRoleFromUserAsync(role.Id, id);
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user {UserId}", id);
                return StatusCode(500, new { error = $"Failed to assign roles to user {id}", message = ex.Message });
            }
        }

        // GET: api/Users/{id}/permissions
        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(id);
                var permissionCodes = permissions.Select(p => p.Code).ToList();

                return Ok(permissionCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for user {UserId}", id);
                return StatusCode(500, new { error = $"Failed to get permissions for user {id}", message = ex.Message });
            }
        }

        // GET: api/Users/{id}/roles
        [HttpGet("{id}/roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _roleRepository.GetUserRolesAsync(id);
                var roleNames = roles.Select(r => r.Name ?? string.Empty).ToList();

                return Ok(roleNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", id);
                return StatusCode(500, new { error = $"Failed to get roles for user {id}", message = ex.Message });
            }
        }
    }
}
