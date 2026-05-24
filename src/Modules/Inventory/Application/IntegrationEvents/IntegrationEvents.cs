using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Inventory.Application.IntegrationEvents
{
    public record ProductStockUpdatedEvent(string Sku, int NewStock);
}
