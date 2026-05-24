using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly GetInventoryStatusUseCase _getInventoryStatus;

        public InventoryController(GetInventoryStatusUseCase getInventoryStatus)
        {
            _getInventoryStatus = getInventoryStatus;
        }

        [HttpGet("{sku}")]
        public async Task<ActionResult<InventoryStatusDto>> GetInventory(string sku)
        {
            var status = await _getInventoryStatus.ExecuteAsync(sku);
            if (status == null)
                return NotFound(new { message = $"SKU {sku} no localizado en inventario." });
            return Ok(status);
        }
    }
}