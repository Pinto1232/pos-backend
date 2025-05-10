using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosBackend.Application.Dtos.Permissions;
using PosBackend.Application.Interfaces;
using PosBackend.Models;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IPermissionRepository permissionRepository,
            ILogger<PermissionsController> logger)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        // GET: api/Permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
        {
            try
            {
                var permissions = await _permissionRepository.GetAllAsync();
                var permissionDtos = permissions.Select(MapToDto).ToList();
                return Ok(permissionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions");
                return StatusCode(500, new { error = "Failed to retrieve permissions", message = ex.Message });
            }
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetPermission(int id)
        {
            try
            {
                var permission = await _permissionRepository.GetByIdAsync(id);
                if (permission == null)
                {
                    return NotFound();
                }

                return Ok(MapToDto(permission));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission {PermissionId}", id);
                return StatusCode(500, new { error = $"Failed to retrieve permission {id}", message = ex.Message });
            }
        }

        // GET: api/Permissions/modules
        [HttpGet("modules")]
        public async Task<ActionResult<IEnumerable<string>>> GetModules()
        {
            try
            {
                var modules = await _permissionRepository.GetModulesAsync();
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permission modules");
                return StatusCode(500, new { error = "Failed to retrieve permission modules", message = ex.Message });
            }
        }

        // GET: api/Permissions/module/{module}
        [HttpGet("module/{module}")]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissionsByModule(string module)
        {
            try
            {
                var permissions = await _permissionRepository.GetByModuleAsync(module);
                var permissionDtos = permissions.Select(MapToDto).ToList();
                return Ok(permissionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting permissions for module {Module}", module);
                return StatusCode(500, new { error = $"Failed to retrieve permissions for module {module}", message = ex.Message });
            }
        }

        // POST: api/Permissions
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PermissionDto>> CreatePermission(PermissionDto permissionDto)
        {
            try
            {
                // Check if permission with same code already exists
                var existingPermission = await _permissionRepository.GetByCodeAsync(permissionDto.Code);
                if (existingPermission != null)
                {
                    return Conflict(new { error = $"Permission with code '{permissionDto.Code}' already exists" });
                }

                var permission = new Permission
                {
                    Name = permissionDto.Name,
                    Code = permissionDto.Code,
                    Description = permissionDto.Description,
                    Module = permissionDto.Module,
                    IsActive = permissionDto.IsActive
                };

                var createdPermission = await _permissionRepository.CreateAsync(permission);
                return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.Id }, MapToDto(createdPermission));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, new { error = "Failed to create permission", message = ex.Message });
            }
        }

        // PUT: api/Permissions/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdatePermission(int id, PermissionDto permissionDto)
        {
            if (id != permissionDto.Id)
            {
                return BadRequest(new { error = "Permission ID mismatch" });
            }

            try
            {
                var permission = await _permissionRepository.GetByIdAsync(id);
                if (permission == null)
                {
                    return NotFound();
                }

                // Check if updating code to one that already exists
                if (permission.Code != permissionDto.Code)
                {
                    var existingPermission = await _permissionRepository.GetByCodeAsync(permissionDto.Code);
                    if (existingPermission != null && existingPermission.Id != id)
                    {
                        return Conflict(new { error = $"Permission with code '{permissionDto.Code}' already exists" });
                    }
                }

                permission.Name = permissionDto.Name;
                permission.Code = permissionDto.Code;
                permission.Description = permissionDto.Description;
                permission.Module = permissionDto.Module;
                permission.IsActive = permissionDto.IsActive;

                var result = await _permissionRepository.UpdateAsync(permission);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating permission {PermissionId}", id);
                return StatusCode(500, new { error = $"Failed to update permission {id}", message = ex.Message });
            }
        }

        // DELETE: api/Permissions/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            try
            {
                var result = await _permissionRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting permission {PermissionId}", id);
                return StatusCode(500, new { error = $"Failed to delete permission {id}", message = ex.Message });
            }
        }

        private static PermissionDto MapToDto(Permission permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Module = permission.Module,
                IsActive = permission.IsActive
            };
        }
    }
}
