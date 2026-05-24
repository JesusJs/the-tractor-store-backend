using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Inventory.Application.UseCase
{
    public class DeductStockOnOrderPlacedUseCase
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILogger<DeductStockOnOrderPlacedUseCase> _logger;

        public DeductStockOnOrderPlacedUseCase(
            IInventoryRepository inventoryRepository,
            ILogger<DeductStockOnOrderPlacedUseCase> logger)
        {
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        public async Task HandleAsync(OrderPlacedEvent @event)
        {
            _logger.LogInformation("Procesando descuento de inventario para la Orden: {OrderId}", @event.OrderId);

            try
            {
                foreach (var item in @event.Items)
                {
                    // Buscamos el stock actual del SKU en la DB aislada de inventario
                    var stock = await _inventoryRepository.GetBySkuAsync(item.Sku);

                    if (stock == null)
                    {
                        _logger.LogError("Error de consistencia: Se intentó descontar stock para el SKU {Sku} pero no existe en el inventario.", item.Sku);
                        throw new InvalidOperationException($"El SKU {item.Sku} no está registrado en el sistema de inventario.");
                    }

                    // Ejecutamos la regla de negocio del dominio
                    stock.DeductStock(item.Quantity);

                    // Persistimos los cambios
                    await _inventoryRepository.UpdateAsync(stock);

                    _logger.LogInformation("Stock reducido exitosamente para el SKU: {Sku}. Unidades descontadas: {Quantity}. Stock restante: {Remaining}",
                        item.Sku, item.Quantity, stock.AvailableStock);
                }

                _logger.LogInformation("Todos los ítems de la Orden {OrderId} fueron descontados correctamente del inventario.", @event.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al intentar reducir el inventario para la Orden: {OrderId}", @event.OrderId);
                throw;
            }
        }
    }
}
