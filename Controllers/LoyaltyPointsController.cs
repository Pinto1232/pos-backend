using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoyaltyPointsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public LoyaltyPointsController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/LoyaltyPoints/customer/5
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<LoyaltyPoint>> GetCustomerLoyaltyPoints(int customerId)
        {
            var loyaltyPoint = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerId == customerId);

            if (loyaltyPoint == null)
            {
                return NotFound();
            }

            return loyaltyPoint;
        }

        // POST: api/LoyaltyPoints/earn
        [HttpPost("earn")]
        public async Task<ActionResult<LoyaltyPoint>> EarnPoints([FromBody] LoyaltyPointsRequest request)
        {
            var loyaltyPoint = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerId == request.CustomerId);

            if (loyaltyPoint == null)
            {
                loyaltyPoint = new LoyaltyPoint
                {
                    CustomerId = request.CustomerId,
                    PointsBalance = request.Points,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LoyaltyPoints.Add(loyaltyPoint);
            }
            else
            {
                loyaltyPoint.PointsBalance += request.Points;
                loyaltyPoint.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(loyaltyPoint);
        }

        // POST: api/LoyaltyPoints/redeem
        [HttpPost("redeem")]
        public async Task<ActionResult<LoyaltyPoint>> RedeemPoints([FromBody] LoyaltyPointsRequest request)
        {
            var loyaltyPoint = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(lp => lp.CustomerId == request.CustomerId);

            if (loyaltyPoint == null)
            {
                return NotFound("No loyalty points found for this customer");
            }

            if (loyaltyPoint.PointsBalance < request.Points)
            {
                return BadRequest("Insufficient loyalty points");
            }

            loyaltyPoint.PointsBalance -= request.Points;
            loyaltyPoint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(loyaltyPoint);
        }
    }

    // DTO for loyalty points operations
    public class LoyaltyPointsRequest
    {
        public int CustomerId { get; set; }
        public int Points { get; set; }
    }
}
