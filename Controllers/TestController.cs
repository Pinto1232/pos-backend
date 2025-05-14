using Microsoft.AspNetCore.Mvc;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// A simple test endpoint to verify the API is working
        /// </summary>
        /// <returns>A simple message</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is working correctly!" });
        }
    }
}
