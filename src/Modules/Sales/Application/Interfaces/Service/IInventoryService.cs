using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Sales.Application.Interfaces.Service
{
    public interface IInventoryService
    {
        Task<bool> DecreaseStockAsync(string sku, int quantity);
    }
}
