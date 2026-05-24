using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private static readonly Dictionary<string, int> MockStock = new()
        {
            { "TX-001-GPS", 8 },
            { "TX-001-AI", 3 },
            { "TX-CLS-01", 0 } // Provoca el estado "Out of stock" en tu front
        };

        [HttpGet("{sku}")]
        public ActionResult<InventoryStatusDto> GetInventory(string sku)
        {
            if (!MockStock.TryGetValue(sku, out var stock))
            {
                return NotFound(new { message = $"SKU {sku} no localizado en inventario." });
            }

            return Ok(new InventoryStatusDto(sku, stock));
        }
    }
}