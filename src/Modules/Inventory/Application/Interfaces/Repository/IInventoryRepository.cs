using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Inventory.Domain.Entities;

namespace TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository
{
    public interface IInventoryRepository
    {
        Task<SkuStock?> GetBySkuAsync(string sku);
        Task UpdateAsync(SkuStock stock);
    }
}
