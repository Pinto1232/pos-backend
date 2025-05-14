using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AddOnsController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly ILogger<AddOnsController> _logger;

        public AddOnsController(PosDbContext context, ILogger<AddOnsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all add-ons with optional filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAddOns(
            [FromQuery] string? category = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (_context.AddOns == null)
                {
                    return NotFound("Add-ons not found");
                }

                // Start with all add-ons
                var query = _context.AddOns.AsQueryable();

                // Apply filters if provided
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(a => a.Category == category);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(a => a.IsActive == isActive.Value);
                }

                // Get total count for pagination
                var totalItems = await query.CountAsync();

                // Apply pagination
                var addOns = await query
                    .OrderBy(a => a.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    totalItems,
                    data = addOns
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAddOns");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific add-on by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AddOn>> GetAddOn(int id)
        {
            try
            {
                var addOn = await _context.AddOns.FindAsync(id);

                if (addOn == null)
                {
                    return NotFound($"Add-on with ID {id} not found");
                }

                return Ok(addOn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving add-on with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new add-on
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<AddOn>> CreateAddOn([FromBody] AddOnDto addOnDto)
        {
            try
            {
                // Create new add-on
                var newAddOn = new AddOn
                {
                    Name = addOnDto.Name,
                    Description = addOnDto.Description,
                    Price = addOnDto.Price,
                    Currency = addOnDto.Currency,
                    MultiCurrencyPrices = addOnDto.MultiCurrencyPrices,
                    Category = addOnDto.Category,
                    IsActive = addOnDto.IsActive,
                    Features = addOnDto.Features,
                    Dependencies = addOnDto.Dependencies,
                    Icon = addOnDto.Icon
                };

                _context.AddOns.Add(newAddOn);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetAddOn),
                    new { id = newAddOn.Id },
                    newAddOn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating add-on");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing add-on
        /// </summary>
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAddOn(int id, [FromBody] AddOnDto addOnDto)
        {
            try
            {
                var addOn = await _context.AddOns.FindAsync(id);

                if (addOn == null)
                {
                    return NotFound($"Add-on with ID {id} not found");
                }

                // Update add-on properties
                addOn.Name = addOnDto.Name;
                addOn.Description = addOnDto.Description;
                addOn.Price = addOnDto.Price;
                addOn.Currency = addOnDto.Currency;
                addOn.MultiCurrencyPrices = addOnDto.MultiCurrencyPrices;
                addOn.Category = addOnDto.Category;
                addOn.IsActive = addOnDto.IsActive;
                addOn.Features = addOnDto.Features;
                addOn.Dependencies = addOnDto.Dependencies;
                addOn.Icon = addOnDto.Icon;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating add-on with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an add-on
        /// </summary>
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteAddOn(int id)
        {
            try
            {
                var addOn = await _context.AddOns.FindAsync(id);

                if (addOn == null)
                {
                    return NotFound($"Add-on with ID {id} not found");
                }

                _context.AddOns.Remove(addOn);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting add-on with ID {id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all available categories
        /// </summary>
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _context.AddOns
                    .Where(a => !string.IsNullOrEmpty(a.Category))
                    .Select(a => a.Category)
                    .Distinct()
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving add-on categories");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// DTO for add-on creation and updates
        /// </summary>
        public class AddOnDto
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;

            [Required]
            public decimal Price { get; set; }

            public string Currency { get; set; } = "USD";

            public string MultiCurrencyPrices { get; set; } = "{}";

            public string Category { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;

            // JSON string to store specific capabilities/functionalities that the addon enables
            public string Features { get; set; } = "[]";

            // JSON string to store any requirements or prerequisites needed for the addon to function
            public string Dependencies { get; set; } = "[]";

            // Icon or visual indicator for the addon (can be a URL or a class name)
            public string Icon { get; set; } = string.Empty;
        }
    }
}
