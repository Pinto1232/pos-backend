using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public CouponsController(PosDbContext context)
        {
            _context = context;
        }

        // Validate Coupon
        [HttpGet("validate/{code}")]
        public async Task<ActionResult<Coupon>> ValidateCoupon(string code)
        {
            var coupon = await _context.Coupons
                .Include(c => c.Discount)
                .FirstOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
            {
                return NotFound("Coupon not found");
            }

            // Check usage limit
            if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
            {
                return BadRequest("Coupon has reached its usage limit");
            }

            // Check discount active status
            if (coupon.Discount.ActiveUntil.HasValue &&
                coupon.Discount.ActiveUntil.Value < DateTime.UtcNow)
            {
                return BadRequest("Coupon has expired");
            }

            return Ok(coupon);
        }

        // Apply Coupon to Sale
        [HttpPost("apply")]
        public async Task<ActionResult<decimal>> ApplyCoupon([FromBody] CouponApplicationRequest request)
        {
            var coupon = await _context.Coupons
                .Include(c => c.Discount)
                .FirstOrDefaultAsync(c => c.Code == request.CouponCode);

            if (coupon == null)
            {
                return NotFound("Coupon not found");
            }

            // Validate coupon
            if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
            {
                return BadRequest("Coupon has reached its usage limit");
            }

            // Calculate discount
            decimal discountAmount = 0;
            switch (coupon.Discount.DiscountType.ToLower())
            {
                case "percentage":
                    discountAmount = request.TotalAmount * (coupon.Discount.Value / 100);
                    break;
                case "fixed":
                    discountAmount = coupon.Discount.Value;
                    break;
            }

            // Increment usage count
            coupon.TimesUsed++;
            await _context.SaveChangesAsync();

            return Ok(discountAmount);
        }
    }

    // DTO for coupon application
    public class CouponApplicationRequest
    {
        public required string CouponCode { get; set; }
        public decimal TotalAmount { get; set; }
    }
}