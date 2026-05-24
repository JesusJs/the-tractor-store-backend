using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Application.Events
{
    public record ProductStockUpdatedEvent(string Sku, int NewStock);
}
