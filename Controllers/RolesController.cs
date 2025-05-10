using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosBackend.Application.Dtos.Roles;
using PosBackend.Application.Interfaces;
using PosBackend.Models;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            ILogger<RolesController> logger)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                var roleDtos = new List<RoleDto>();

                foreach (var role in roles)
                {
                    var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);
                    var userCount = await _roleRepository.GetUserCountByRoleIdAsync(role.Id);

                    roleDtos.Add(new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name ?? string.Empty,
                        Description = role.NormalizedName,
                        IsSystemRole = role.Name?.StartsWith("System_") ?? false,
                        UserCount = userCount,
                        Permissions = permissions.Select(p => p.Code).ToList()
                    });
                }

                return Ok(roleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, new { error = "Failed to retrieve roles", message = ex.Message });
            }
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(id);
                var userCount = await _roleRepository.GetUserCountByRoleIdAsync(id);

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Description = role.NormalizedName,
                    IsSystemRole = role.Name?.StartsWith("System_") ?? false,
                    UserCount = userCount,
                    Permissions = permissions.Select(p => p.Code).ToList()
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role {RoleId}", id);
                return StatusCode(500, new { error = $"Failed to retrieve role {id}", message = ex.Message });
            }
        }

        // POST: api/Roles
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<RoleDto>> CreateRole(RoleDto roleDto)
        {
            try
            {
                // Check if role with same name already exists
                var existingRole = await _roleRepository.GetByNameAsync(roleDto.Name);
                if (existingRole != null)
                {
                    return Conflict(new { error = $"Role with name '{roleDto.Name}' already exists" });
                }

                var role = new UserRole
                {
                    Name = roleDto.Name,
                    NormalizedName = roleDto.Description,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    UserRoles = new List<UserRoleMapping>(),
                    RolePermissions = new List<RolePermission>()
                };

                var createdRole = await _roleRepository.CreateAsync(role);

                // Assign permissions if any
                if (roleDto.Permissions != null && roleDto.Permissions.Any())
                {
                    foreach (var permissionCode in roleDto.Permissions)
                    {
                        var permission = await _permissionRepository.GetByCodeAsync(permissionCode);
                        if (permission != null)
                        {
                            await _roleRepository.AssignPermissionToRoleAsync(createdRole.Id, permission.Id);
                        }
                    }
                }

                // Get updated role with permissions
                return await GetRole(createdRole.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, new { error = "Failed to create role", message = ex.Message });
            }
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateRole(int id, RoleDto roleDto)
        {
            if (id != roleDto.Id)
            {
                return BadRequest(new { error = "Role ID mismatch" });
            }

            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Check if updating name to one that already exists
                if (role.Name != roleDto.Name)
                {
                    var existingRole = await _roleRepository.GetByNameAsync(roleDto.Name);
                    if (existingRole != null && existingRole.Id != id)
                    {
                        return Conflict(new { error = $"Role with name '{roleDto.Name}' already exists" });
                    }
                }

                // Don't allow modifying system roles
                if (role.Name?.StartsWith("System_") == true && !roleDto.Name.StartsWith("System_"))
                {
                    return BadRequest(new { error = "Cannot modify system roles" });
                }

                role.Name = roleDto.Name;
                role.NormalizedName = roleDto.Description;

                var result = await _roleRepository.UpdateAsync(role);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {RoleId}", id);
                return StatusCode(500, new { error = $"Failed to update role {id}", message = ex.Message });
            }
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Don't allow deleting system roles
                if (role.Name?.StartsWith("System_") == true)
                {
                    return BadRequest(new { error = "Cannot delete system roles" });
                }

                var result = await _roleRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {RoleId}", id);
                return StatusCode(500, new { error = $"Failed to delete role {id}", message = ex.Message });
            }
        }

        // POST: api/Roles/5/permissions
        [HttpPost("{id}/permissions")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignPermissionsToRole(int id, [FromBody] List<string> permissionCodes)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Get current permissions
                var currentPermissions = await _permissionRepository.GetPermissionsByRoleIdAsync(id);
                var currentPermissionCodes = currentPermissions.Select(p => p.Code).ToList();

                // Permissions to add
                var permissionsToAdd = permissionCodes.Except(currentPermissionCodes).ToList();

                // Permissions to remove
                var permissionsToRemove = currentPermissionCodes.Except(permissionCodes).ToList();

                // Add new permissions
                foreach (var code in permissionsToAdd)
                {
                    var permission = await _permissionRepository.GetByCodeAsync(code);
                    if (permission != null)
                    {
                        await _roleRepository.AssignPermissionToRoleAsync(id, permission.Id);
                    }
                }

                // Remove permissions
                foreach (var code in permissionsToRemove)
                {
                    var permission = await _permissionRepository.GetByCodeAsync(code);
                    if (permission != null)
                    {
                        await _roleRepository.RemovePermissionFromRoleAsync(id, permission.Id);
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permissions to role {RoleId}", id);
                return StatusCode(500, new { error = $"Failed to assign permissions to role {id}", message = ex.Message });
            }
        }
    }
}
