using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;

namespace TractorEcommerce.Api.Extensions
{
    public class SqlInventoryService : IInventoryService
    {
        private readonly CatalogDbContext _catalogDbContext;

        public SqlInventoryService(CatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;
        }

        public async Task<bool> DecreaseStockAsync(string sku, int quantity)
        {
            var variant = await _catalogDbContext.ProductVariants
                .FirstOrDefaultAsync(v => v.Sku.ToUpper() == sku.ToUpper());

            if (variant == null || variant.Stock < quantity)
            {
                return false;
            }

            variant.UpdateStock(variant.Stock - quantity);
            await _catalogDbContext.SaveChangesAsync();
            return true;
        }
    }
}
