using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosBackend.Models;
using System.Threading.Tasks;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly PosDbContext _context;

        public InventoryMovementsController(PosDbContext context)
        {
            _context = context;
        }

        // GET: api/InventoryMovements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetInventoryMovements()
        {
            return await _context.InventoryMovements
                .Include(im => im.ProductVariant)
                .Include(im => im.Store)
                .ToListAsync();
        }

        // GET: api/InventoryMovements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryMovement>> GetInventoryMovement(int id)
        {
            var inventoryMovement = await _context.InventoryMovements
                .Include(im => im.ProductVariant)
                .Include(im => im.Store)
                .FirstOrDefaultAsync(im => im.MovementId == id);

            if (inventoryMovement == null)
            {
                return NotFound();
            }

            return inventoryMovement;
        }

        // POST: api/InventoryMovements
        [HttpPost]
        public async Task<ActionResult<InventoryMovement>> CreateInventoryMovement(InventoryMovement inventoryMovement)
        {
            // Set timestamp
            inventoryMovement.Timestamp = DateTime.UtcNow;

            // Update inventory based on movement type
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i =>
                    i.StoreId == inventoryMovement.StoreId &&
                    i.VariantId == inventoryMovement.VariantId);

            if (inventory == null)
            {
                return BadRequest("Inventory record not found");
            }

            // Adjust inventory quantity based on movement type
            switch (inventoryMovement.Type.ToLower())
            {
                case "stock_in":
                    inventory.Quantity += inventoryMovement.Quantity;
                    break;
                case "stock_out":
                    if (inventory.Quantity < inventoryMovement.Quantity)
                    {
                        return BadRequest("Insufficient inventory");
                    }
                    inventory.Quantity -= inventoryMovement.Quantity;
                    break;
            }

            inventory.LastUpdated = DateTime.UtcNow;

            _context.InventoryMovements.Add(inventoryMovement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventoryMovement), new { id = inventoryMovement.MovementId }, inventoryMovement);
        }

        // GET: api/InventoryMovements/product/{productVariantId}
        [HttpGet("product/{productVariantId}")]
        public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetMovementsByProduct(int productVariantId)
        {
            return await _context.InventoryMovements
                .Where(im => im.VariantId == productVariantId)
                .Include(im => im.ProductVariant)
                .Include(im => im.Store)
                .OrderByDescending(im => im.Timestamp)
                .ToListAsync();
        }
    }
}
