using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PosBackend.Models;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private const string InternalServerErrorMessage = "Internal server error";
        
        private readonly PosDbContext _context;
        private readonly ILogger<BillingController> _logger;

        public BillingController(
            PosDbContext context,
            ILogger<BillingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Billing/user/{userId}/history
        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<List<BillingHistoryItem>>> GetBillingHistory(string userId)
        {
            try
            {
                var stripeSubscriptions = await _context.StripeSubscriptions
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                var billingHistory = stripeSubscriptions.Select(s => new BillingHistoryItem
                {
                    Date = s.LatestInvoiceDate ?? s.CreatedAt,
                    Amount = s.LatestInvoiceAmount ?? s.Amount,
                    Currency = s.Currency,
                    Status = s.LatestInvoiceStatus ?? "pending",
                    InvoiceId = s.LatestInvoiceId,
                    Description = $"Subscription payment - {s.BillingInterval}ly"
                }).ToList();

                return Ok(billingHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving billing history for user {UserId}", userId);
                return StatusCode(500, new { error = InternalServerErrorMessage });
            }
        }
    }

    public class BillingHistoryItem
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
