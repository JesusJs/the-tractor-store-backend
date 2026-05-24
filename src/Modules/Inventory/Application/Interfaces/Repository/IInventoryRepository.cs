using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Inventory.Domain.Entities;

namespace TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository
{
    public interface IInventoryRepository
    {
        Task UpdateAsync(InventoryItem stock);

        Task<InventoryItem?> GetBySkuAsync(string sku);

        Task UpsertStockAsync(string sku, int quantity);
    }
}
