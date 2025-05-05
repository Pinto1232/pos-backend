using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PosBackend.Hubs;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserCustomizationController : ControllerBase
    {
        private readonly PosDbContext _context;
        private readonly IHubContext<CustomizationHub> _hubContext;

        public UserCustomizationController(PosDbContext context, IHubContext<CustomizationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/UserCustomization/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCustomization(string userId)
        {
            var customization = await _context.UserCustomizations.FirstOrDefaultAsync(x => x.UserId == userId);
            if (customization == null)
            {
                return NotFound();
            }
            return Ok(customization);
        }

        // POST: api/UserCustomization
        [HttpPost]
        public async Task<IActionResult> UpdateCustomization([FromBody] UserCustomization customization)
        {
            var existing = await _context.UserCustomizations.FirstOrDefaultAsync(x => x.UserId == customization.UserId);
            if (existing == null)
            {
                _context.UserCustomizations.Add(customization);
            }
            else
            {
                existing.SidebarColor = customization.SidebarColor;
                existing.LogoUrl = customization.LogoUrl;
                existing.NavbarColor = customization.NavbarColor;

                // Update tax settings if provided
                if (customization.TaxSettings != null)
                {
                    existing.TaxSettings = customization.TaxSettings;
                }

                // Update regional settings if provided
                if (customization.RegionalSettings != null)
                {
                    existing.RegionalSettings = customization.RegionalSettings;
                }
            }
            await _context.SaveChangesAsync();

            // Broadcast update via SignalR to the specific user (assuming SignalR is set up to use user identifiers)
            await _hubContext.Clients.User(customization.UserId)
                .SendAsync("ReceiveCustomizationUpdate", customization);

            return Ok(customization);
        }
    }
}
