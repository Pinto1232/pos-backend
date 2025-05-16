using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint to verify the API is running
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check endpoint called at {time}", DateTime.UtcNow);
            
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }
    }
}
