using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Application.Ports
{
    public interface ICatalogEventPublisher
    {
        Task PublishStockUpdatedAsync(string sku, int newStock);
    }
}
