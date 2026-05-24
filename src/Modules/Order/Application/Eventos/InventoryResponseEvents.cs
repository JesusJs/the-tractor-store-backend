using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Order.Application.Eventos
{
    // El evento que llega si el inventario se descontó con éxito
    public record InventoryReservedIntegrationEvent(Guid OrderId, DateTime ReservedAt);

    // El evento que llega si no había stock físico suficiente
    public record InventoryReservationFailedIntegrationEvent(Guid OrderId, string Reason, DateTime FailedAt);
}
