using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        // Asumiendo el caso de uso de lectura que expone tu controlador actual
        private readonly GetInventoryStatusUseCase _getInventoryStatus;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            GetInventoryStatusUseCase getInventoryStatus,
            ILogger<InventoryController> logger)
        {
            _getInventoryStatus = getInventoryStatus;
            _logger = logger;
        }

        [HttpGet("{sku}")]
        public async Task<ActionResult<object>> GetInventory(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("El SKU es obligatorio.", nameof(sku));

            _logger.LogInformation("Consultando estado de inventario para el SKU: {Sku}", sku);

            var status = await _getInventoryStatus.ExecuteAsync(sku);

            if (status == null)
            {
                _logger.LogWarning("Consulta fallida: El SKU {Sku} no existe en el inventario.", sku);
                throw new DomainNotFoundException($"El SKU '{sku}' no fue localizado en el inventario.");
            }

            return Ok(status);
        }
    }
}