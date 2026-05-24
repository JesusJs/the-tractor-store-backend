using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Domain.Entities;

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Persistence
{
    public class SqlCatalogRepository : ICatalogRepository
    {
        private readonly CatalogDbContext _context;

        public SqlCatalogRepository(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            if (string.Equals(category, "all", StringComparison.OrdinalIgnoreCase))
            {
                return await _context.Products
                    .Include(p => p.Variants)
                    .ToListAsync();
            }

            var categoryLower = category.ToLower();
            return await _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Category == categoryLower)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            return await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ProductVariant?> GetVariantBySkuAsync(string sku)
        {
            return await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Sku == sku);
        }

        public async Task<IEnumerable<string>> GetActiveCategoriesAsync()
        {
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Store>> GetStoresAsync()
        {
            return await _context.Stores.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBySkusAsync(IEnumerable<string> skus)
        {
            var skuList = skus.Select(s => s.ToUpper()).ToList();

            // Buscamos los productos cuyas variantes tengan alguno de los SKUs
            var productIds = await _context.ProductVariants
                .Where(v => skuList.Contains(v.Sku.ToUpper()))
                .Select(v => v.ProductId)
                .Distinct()
                .ToListAsync();

            return await _context.Products
                .Include(p => p.Variants)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
    }
}
