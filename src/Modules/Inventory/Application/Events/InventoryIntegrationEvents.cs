using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Inventory.Application.Events
{
    // 1. El evento que viene desde el módulo de Órdenes (Debe calzar con el JSON enviado)
    public record OrderPlacedIntegrationEvent(Guid OrderId, string CustomerId, List<OrderStockItemDto> Items);
    public record OrderStockItemDto(string Sku, int Quantity);

    // 2. El evento que emitiremos si TODO SALE BIEN
    public record InventoryReservedIntegrationEvent(Guid OrderId, DateTime ReservedAt);

    // 3. El evento que emitiremos si NO HAY SUFICIENTES TRACTORES/REPUESTOS
    public record InventoryReservationFailedIntegrationEvent(Guid OrderId, string Reason, DateTime FailedAt);
}
