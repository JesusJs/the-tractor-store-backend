using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly ICatalogRepository _catalogRepository;

        public InventoryController(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        [HttpGet("{sku}")]
        public async Task<ActionResult<InventoryStatusDto>> GetInventory(string sku)
        {
            var variant = await _catalogRepository.GetVariantBySkuAsync(sku);
            if (variant == null)
            {
                return NotFound(new { message = $"SKU {sku} no localizado en inventario." });
            }

            return Ok(new InventoryStatusDto(sku, variant.Stock));
        }
    }
}