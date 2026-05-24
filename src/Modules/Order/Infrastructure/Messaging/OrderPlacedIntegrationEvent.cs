using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Order.Infrastructure.Messaging
{
    // El evento que anuncia a Kafka que se ha generado una orden
    public record OrderPlacedIntegrationEvent(
        Guid OrderId,
        string CustomerId,
        List<OrderStockItemDto> Items
    );

    // Representa cada tractor o ítem que se compró
    public record OrderStockItemDto(
        string Sku,
        int Quantity
    );
}
