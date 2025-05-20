using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using PosBackend.Services;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSubscriptionController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly PackageFeatureService _packageFeatureService;

        public UserSubscriptionController(PosDbContext context, PackageFeatureService packageFeatureService)
        {
            _context = context;
            _packageFeatureService = packageFeatureService;
        }

        // GET: api/UserSubscription/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserSubscription>> GetUserSubscription(string userId)
        {
            var subscription = await _packageFeatureService.GetUserActiveSubscription(userId);

            if (subscription == null)
            {
                return NotFound("No active subscription found for this user");
            }

            return Ok(subscription);
        }

        // GET: api/UserSubscription/user/{userId}/features
        [HttpGet("user/{userId}/features")]
        public async Task<ActionResult<List<string>>> GetUserFeatures(string userId)
        {
            var features = await _packageFeatureService.GetUserAvailableFeatures(userId);
            return Ok(features);
        }

        // POST: api/UserSubscription/create
        [HttpPost("create")]
        public async Task<ActionResult<UserSubscription>> CreateSubscription([FromBody] UserSubscription subscription)
        {
            // Check if user already has an active subscription
            var existingSubscription = await _packageFeatureService.GetUserActiveSubscription(subscription.UserId);

            if (existingSubscription != null)
            {
                // Deactivate existing subscription
                existingSubscription.IsActive = false;
                existingSubscription.EndDate = DateTime.UtcNow;
                _context.Entry(existingSubscription).State = EntityState.Modified;
            }

            // Set up the new subscription
            subscription.StartDate = DateTime.UtcNow;
            subscription.IsActive = true;
            subscription.CreatedAt = DateTime.UtcNow;
            subscription.LastUpdatedAt = DateTime.UtcNow;

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserSubscription), new { userId = subscription.UserId }, subscription);
        }

        // POST: api/UserSubscription/user/{userId}/enable-package/{packageId}
        [HttpPost("user/{userId}/enable-package/{packageId}")]
        public async Task<ActionResult> EnableAdditionalPackage(string userId, int packageId)
        {
            var package = await _context.PricingPackages.FindAsync(packageId);
            if (package == null)
            {
                return NotFound("Package not found");
            }

            var result = await _packageFeatureService.EnableAdditionalPackage(userId, packageId);
            if (!result)
            {
                return NotFound("No active subscription found for this user");
            }

            return Ok(new { message = $"Package '{package.Title}' enabled successfully" });
        }

        // POST: api/UserSubscription/user/{userId}/disable-package/{packageId}
        [HttpPost("user/{userId}/disable-package/{packageId}")]
        public async Task<ActionResult> DisableAdditionalPackage(string userId, int packageId)
        {
            var package = await _context.PricingPackages.FindAsync(packageId);
            if (package == null)
            {
                return NotFound("Package not found");
            }

            var result = await _packageFeatureService.DisableAdditionalPackage(userId, packageId);
            if (!result)
            {
                return NotFound("No active subscription found for this user");
            }

            return Ok(new { message = $"Package '{package.Title}' disabled successfully" });
        }

        // GET: api/UserSubscription/check-access/{userId}/{featureName}
        [HttpGet("check-access/{userId}/{featureName}")]
        public async Task<ActionResult<bool>> CheckFeatureAccess(string userId, string featureName)
        {
            var hasAccess = await _packageFeatureService.UserHasFeatureAccess(userId, featureName);
            return Ok(hasAccess);
        }
    }
}
