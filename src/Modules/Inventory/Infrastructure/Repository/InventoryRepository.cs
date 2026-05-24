using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Inventory.Domain.Entities;
using TractorEcommerce.Modules.Inventory.Infrastructure.Data;

namespace TractorEcommerce.Modules.Inventory.Infrastructure.Repository
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<SkuStock?> GetBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return null;

            // Convertimos a Mayúsculas para asegurar consistencia
            return await _context.Stocks
                .FirstOrDefaultAsync(s => s.Sku == sku.ToUpper());
        }

        public async Task UpdateAsync(SkuStock stock)
        {
            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync();
        }
    }
}
