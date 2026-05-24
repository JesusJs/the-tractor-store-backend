using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Sales.Application.Interfaces;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Infrastructure.Repository
{
    // Adaptador simulado para la transacción de Base de Datos
    public class MemoryDbTransactionWrapper : IDbTransactionWrapper
    {
        public Task CommitAsync() => Task.CompletedTask;
        public Task RollbackAsync() => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    // Adaptador simulado para el repositorio de Ventas
    public class MemorySalesRepository : ISalesRepository
    {
        private static readonly Dictionary<string, Cart> _carts = new();

        public Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            if (!_carts.TryGetValue(userId, out var cart))
            {
                cart = new Cart(userId);
                _carts[userId] = cart;
            }
            return Task.FromResult<Cart?>(cart);
        }

        public Task SaveCartAsync(Cart cart)
        {
            _carts[cart.UserId] = cart;
            return Task.CompletedTask;
        }

        public Task SaveOrderReceiptAsync(OrderReceiptDto order)
        {
            // Aquí se guardaría en la tabla sales.orders en Postgres
            return Task.CompletedTask;
        }

        public Task<OrderReceiptDto?> GetOrderByIdAsync(string id)
        {
            return Task.FromResult<OrderReceiptDto?>(null);
        }

        public Task<IDbTransactionWrapper> BeginTransactionAsync()
        {
            return Task.FromResult<IDbTransactionWrapper>(new MemoryDbTransactionWrapper());
        }
    }

    // Adaptador simulado para comunicar las ventas con el Stock de Inventario
    public class MemoryInventoryService : IInventoryService
    {
        public Task<bool> DecreaseStockAsync(string sku, int quantity)
        {
            // Requisito del front: Si es el SKU agotado simulado, devuelve false
            if (sku == "TX-CLS-01") return Task.FromResult(false);

            // De lo contrario, simula que siempre hay stock disponible para la compra
            return Task.FromResult(true);
        }
    }
}
