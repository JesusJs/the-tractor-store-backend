using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly GetInventoryStatusQueryHandler _getInventoryStatusHandler;

        public InventoryController(GetInventoryStatusQueryHandler getInventoryStatusHandler)
        {
            _getInventoryStatusHandler = getInventoryStatusHandler;
        }

        [HttpGet("{sku}")]
        public async Task<ActionResult<InventoryStatusDto>> GetInventory(string sku)
        {
            var status = await _getInventoryStatusHandler.ExecuteAsync(sku);
            if (status == null)
            {
                return NotFound(new { message = $"SKU {sku} no localizado en inventario." });
            }

            return Ok(status);
        }
    }
}