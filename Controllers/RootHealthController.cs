using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PosBackend.Controllers
{
    [Route("health")]
    [ApiController]
    [AllowAnonymous]
    public class RootHealthController : ControllerBase
    {
        private readonly ILogger<RootHealthController> _logger;

        public RootHealthController(ILogger<RootHealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Root health check endpoint called at {time}", DateTime.UtcNow);
            
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }
    }
}
