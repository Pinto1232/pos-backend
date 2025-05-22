using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using POS.Models;
using POS.DTOs;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentPlansController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly ILogger<PaymentPlansController> _logger;

        public PaymentPlansController(PosDbContext context, ILogger<PaymentPlansController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all available payment plans with optional filtering
        /// </summary>
        /// <param name="currency">Filter by currency (default: USD)</param>
        /// <param name="region">Filter by region</param>
        /// <param name="userType">Filter by user type</param>
        /// <param name="includeInactive">Include inactive plans (default: false)</param>
        /// <returns>List of payment plans</returns>
        [HttpGet]
        public async Task<ActionResult<PaymentPlansResponse>> GetPaymentPlans(
            [FromQuery] string currency = "USD",
            [FromQuery] string? region = null,
            [FromQuery] string? userType = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Fetching payment plans for currency: {Currency}, region: {Region}, userType: {UserType}", 
                    currency, region, userType);

                var query = _context.PaymentPlans.AsQueryable();

                // Filter by currency
                query = query.Where(p => p.Currency == currency);

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(p => p.IsActive && p.IsCurrentlyValid());
                }

                var allPlans = await query.OrderBy(p => p.Id).ToListAsync();

                // Apply region and user type filters using helper methods
                var filteredPlans = allPlans
                    .Where(p => p.AppliesToRegion(region) && p.AppliesToUserType(userType))
                    .ToList();

                var planDtos = filteredPlans.Select(p => new PaymentPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Period = p.Period,
                    DiscountPercentage = p.DiscountPercentage,
                    DiscountLabel = p.DiscountLabel,
                    Description = p.Description,
                    IsPopular = p.IsPopular,
                    IsDefault = p.IsDefault,
                    ValidFrom = p.ValidFrom,
                    ValidTo = p.ValidTo,
                    ApplicableRegions = p.GetApplicableRegions(),
                    ApplicableUserTypes = p.GetApplicableUserTypes(),
                    Currency = p.Currency
                }).ToList();

                var defaultPlan = planDtos.FirstOrDefault(p => p.IsDefault);

                var response = new PaymentPlansResponse
                {
                    Plans = planDtos,
                    DefaultPlanId = defaultPlan?.Id,
                    Currency = currency,
                    TotalCount = planDtos.Count
                };

                _logger.LogInformation("Successfully retrieved {Count} payment plans", planDtos.Count);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment plans");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific payment plan by ID
        /// </summary>
        /// <param name="id">Payment plan ID</param>
        /// <returns>Payment plan details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentPlanDto>> GetPaymentPlan(int id)
        {
            try
            {
                var plan = await _context.PaymentPlans.FindAsync(id);

                if (plan == null)
                {
                    return NotFound(new { error = "Payment plan not found" });
                }

                var planDto = new PaymentPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    Period = plan.Period,
                    DiscountPercentage = plan.DiscountPercentage,
                    DiscountLabel = plan.DiscountLabel,
                    Description = plan.Description,
                    IsPopular = plan.IsPopular,
                    IsDefault = plan.IsDefault,
                    ValidFrom = plan.ValidFrom,
                    ValidTo = plan.ValidTo,
                    ApplicableRegions = plan.GetApplicableRegions(),
                    ApplicableUserTypes = plan.GetApplicableUserTypes(),
                    Currency = plan.Currency
                };

                return Ok(planDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment plan {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new payment plan (Admin only)
        /// </summary>
        /// <param name="request">Payment plan creation request</param>
        /// <returns>Created payment plan</returns>
        [HttpPost]
        public async Task<ActionResult<PaymentPlanDto>> CreatePaymentPlan([FromBody] CreatePaymentPlanRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Period))
                {
                    return BadRequest(new { error = "Name and Period are required" });
                }

                if (request.DiscountPercentage < 0 || request.DiscountPercentage > 1)
                {
                    return BadRequest(new { error = "Discount percentage must be between 0 and 1" });
                }

                // If this is set as default, unset other defaults for the same currency
                if (request.IsDefault)
                {
                    var existingDefaults = await _context.PaymentPlans
                        .Where(p => p.Currency == request.Currency && p.IsDefault)
                        .ToListAsync();

                    foreach (var existing in existingDefaults)
                    {
                        existing.IsDefault = false;
                    }
                }

                var paymentPlan = new PaymentPlan
                {
                    Name = request.Name,
                    Period = request.Period,
                    DiscountPercentage = request.DiscountPercentage,
                    DiscountLabel = request.DiscountLabel,
                    Description = request.Description,
                    IsPopular = request.IsPopular,
                    IsDefault = request.IsDefault,
                    ValidFrom = request.ValidFrom,
                    ValidTo = request.ValidTo,
                    Currency = request.Currency,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                paymentPlan.SetApplicableRegions(request.ApplicableRegions);
                paymentPlan.SetApplicableUserTypes(request.ApplicableUserTypes);

                _context.PaymentPlans.Add(paymentPlan);
                await _context.SaveChangesAsync();

                var planDto = new PaymentPlanDto
                {
                    Id = paymentPlan.Id,
                    Name = paymentPlan.Name,
                    Period = paymentPlan.Period,
                    DiscountPercentage = paymentPlan.DiscountPercentage,
                    DiscountLabel = paymentPlan.DiscountLabel,
                    Description = paymentPlan.Description,
                    IsPopular = paymentPlan.IsPopular,
                    IsDefault = paymentPlan.IsDefault,
                    ValidFrom = paymentPlan.ValidFrom,
                    ValidTo = paymentPlan.ValidTo,
                    ApplicableRegions = paymentPlan.GetApplicableRegions(),
                    ApplicableUserTypes = paymentPlan.GetApplicableUserTypes(),
                    Currency = paymentPlan.Currency
                };

                _logger.LogInformation("Created new payment plan: {Name} with ID: {Id}", paymentPlan.Name, paymentPlan.Id);
                return CreatedAtAction(nameof(GetPaymentPlan), new { id = paymentPlan.Id }, planDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment plan");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing payment plan (Admin only)
        /// </summary>
        /// <param name="id">Payment plan ID</param>
        /// <param name="request">Update request</param>
        /// <returns>Updated payment plan</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentPlanDto>> UpdatePaymentPlan(int id, [FromBody] UpdatePaymentPlanRequest request)
        {
            try
            {
                var paymentPlan = await _context.PaymentPlans.FindAsync(id);

                if (paymentPlan == null)
                {
                    return NotFound(new { error = "Payment plan not found" });
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Name))
                    paymentPlan.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Period))
                    paymentPlan.Period = request.Period;

                if (request.DiscountPercentage.HasValue)
                {
                    if (request.DiscountPercentage < 0 || request.DiscountPercentage > 1)
                    {
                        return BadRequest(new { error = "Discount percentage must be between 0 and 1" });
                    }
                    paymentPlan.DiscountPercentage = request.DiscountPercentage.Value;
                }

                if (request.DiscountLabel != null)
                    paymentPlan.DiscountLabel = request.DiscountLabel;

                if (!string.IsNullOrEmpty(request.Description))
                    paymentPlan.Description = request.Description;

                if (request.IsPopular.HasValue)
                    paymentPlan.IsPopular = request.IsPopular.Value;

                if (request.IsDefault.HasValue)
                {
                    if (request.IsDefault.Value)
                    {
                        // Unset other defaults for the same currency
                        var existingDefaults = await _context.PaymentPlans
                            .Where(p => p.Currency == paymentPlan.Currency && p.IsDefault && p.Id != id)
                            .ToListAsync();

                        foreach (var existing in existingDefaults)
                        {
                            existing.IsDefault = false;
                        }
                    }
                    paymentPlan.IsDefault = request.IsDefault.Value;
                }

                if (request.ValidFrom.HasValue)
                    paymentPlan.ValidFrom = request.ValidFrom.Value;

                if (request.ValidTo.HasValue)
                    paymentPlan.ValidTo = request.ValidTo.Value;

                if (request.ApplicableRegions != null)
                    paymentPlan.SetApplicableRegions(request.ApplicableRegions);

                if (request.ApplicableUserTypes != null)
                    paymentPlan.SetApplicableUserTypes(request.ApplicableUserTypes);

                if (request.IsActive.HasValue)
                    paymentPlan.IsActive = request.IsActive.Value;

                paymentPlan.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var planDto = new PaymentPlanDto
                {
                    Id = paymentPlan.Id,
                    Name = paymentPlan.Name,
                    Period = paymentPlan.Period,
                    DiscountPercentage = paymentPlan.DiscountPercentage,
                    DiscountLabel = paymentPlan.DiscountLabel,
                    Description = paymentPlan.Description,
                    IsPopular = paymentPlan.IsPopular,
                    IsDefault = paymentPlan.IsDefault,
                    ValidFrom = paymentPlan.ValidFrom,
                    ValidTo = paymentPlan.ValidTo,
                    ApplicableRegions = paymentPlan.GetApplicableRegions(),
                    ApplicableUserTypes = paymentPlan.GetApplicableUserTypes(),
                    Currency = paymentPlan.Currency
                };

                _logger.LogInformation("Updated payment plan: {Name} with ID: {Id}", paymentPlan.Name, paymentPlan.Id);
                return Ok(planDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment plan {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a payment plan (Admin only)
        /// </summary>
        /// <param name="id">Payment plan ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePaymentPlan(int id)
        {
            try
            {
                var paymentPlan = await _context.PaymentPlans.FindAsync(id);

                if (paymentPlan == null)
                {
                    return NotFound(new { error = "Payment plan not found" });
                }

                _context.PaymentPlans.Remove(paymentPlan);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted payment plan: {Name} with ID: {Id}", paymentPlan.Name, paymentPlan.Id);
                return Ok(new { message = "Payment plan deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment plan {Id}", id);
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }
}
