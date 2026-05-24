using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Inventory.Application.DTOs;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Inventory.Application.UseCase
{
    public class GetInventoryStatusUseCase
    {
        private readonly IInventoryRepository _inventoryRepository; // <-- Autonómico y desacoplado

        public GetInventoryStatusUseCase(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<InventoryStatusDto?> ExecuteAsync(string sku)
        {
            // Consultamos la tabla local del módulo de inventario
            var stockItem = await _inventoryRepository.GetBySkuAsync(sku);
            if (stockItem == null) return null;

            return new InventoryStatusDto(stockItem.Sku, stockItem.Quantity);
        }
    }
}
