using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Sales.Domain.Events
{
    public record OrderPlacedEvent(
    string OrderId,
    IEnumerable<OrderEventItem> Items,
    DateTime OccurredAt);

    public record OrderEventItem(string Sku, int Quantity);
}
