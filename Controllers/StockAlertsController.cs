using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAlertsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public StockAlertsController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/StockAlerts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAlert>>> GetStockAlerts()
        {
            return await _context.StockAlerts
                .Include(sa => sa.Inventory)
                    .ThenInclude(i => i.ProductVariant)
                .Where(sa => sa.IsActive)
                .ToListAsync();
        }

        // POST: api/StockAlerts
        [HttpPost]
        public async Task<ActionResult<StockAlert>> CreateStockAlert(StockAlert stockAlert)
        {
            // Validate inventory exists
            var inventory = await _context.Inventories.FindAsync(stockAlert.InventoryId);
            if (inventory == null)
            {
                return BadRequest("Inventory not found");
            }

            // Set alert to active by default
            stockAlert.IsActive = true;

            _context.StockAlerts.Add(stockAlert);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStockAlerts), new { id = stockAlert.AlertId }, stockAlert);
        }

        // PUT: api/StockAlerts/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStockAlertStatus(int id, [FromBody] bool isActive)
        {
            var stockAlert = await _context.StockAlerts.FindAsync(id);

            if (stockAlert == null)
            {
                return NotFound();
            }

            stockAlert.IsActive = isActive;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/StockAlerts/check
        [HttpGet("check")]
        public async Task<ActionResult<IEnumerable<StockAlert>>> CheckInventoryLevels()
        {
            // Get all inventories with quantity below reorder level
            var lowStockInventories = await _context.Inventories
                .Where(i => i.Quantity <= i.ReorderLevel)
                .Include(i => i.ProductVariant)
                .ToListAsync();

            // Create or update stock alerts for low stock items
            var stockAlerts = new List<StockAlert>();
            foreach (var inventory in lowStockInventories)
            {
                var existingAlert = await _context.StockAlerts
                    .FirstOrDefaultAsync(sa =>
                        sa.InventoryId == inventory.InventoryId &&
                        sa.AlertType == "Low Stock" &&
                        sa.IsActive);

                if (existingAlert == null)
                {
                    var newAlert = new StockAlert
                    {
                        InventoryId = inventory.InventoryId,
                        Inventory = inventory,  // Add this line to set the required Inventory property
                        AlertType = "Low Stock",
                        Threshold = inventory.ReorderLevel,
                        IsActive = true
                    };
                    _context.StockAlerts.Add(newAlert);
                    stockAlerts.Add(newAlert);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(stockAlerts);
        }
    }
}