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

        // 1. Ahora retorna la entidad rica 'InventoryItem' en lugar de SkuStock
        public async Task<InventoryItem?> GetBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return null;

            var normalizedSku = sku.ToUpper().Trim();

            return await _context.Stocks
                .FirstOrDefaultAsync(s => s.Sku == normalizedSku);
        }

        // 2. Método semántico para actualizar cambios generados por la lógica de dominio (ej: DeductStock)
        public async Task UpdateAsync(InventoryItem stock)
        {
            if (stock == null) throw new ArgumentNullException(nameof(stock));

            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync();
        }

        // 3. ¡EL FALTANTE! El Upsert atómico que procesa los eventos de Kafka
        public async Task UpsertStockAsync(string sku, int quantity)
        {
            if (string.IsNullOrWhiteSpace(sku)) return;

            var normalizedSku = sku.ToUpper().Trim();

            // Buscamos directamente en el DbSet local
            var existingStock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.Sku == normalizedSku);

            if (existingStock == null)
            {
                // Si el catálogo nos avisa de un producto nuevo, lo insertamos usando el constructor rico
                var newStock = new InventoryItem(
                    id: Guid.NewGuid(),
                    sku: normalizedSku,
                    availableStock: quantity
                );
                await _context.Stocks.AddAsync(newStock);
            }
            else
            {
                // Si ya existe, ejecutamos el comportamiento de dominio seguro
                existingStock.UpdateQuantity(quantity);
                _context.Stocks.Update(existingStock);
            }

            await _context.SaveChangesAsync();
        }
    }
}
