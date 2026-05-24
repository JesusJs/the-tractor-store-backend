using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TractorEcommerce.Modules.Sales.Application.Interfaces;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using TractorEcommerce.Modules.Sales.Infrastructure.Persistence;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Infrastructure.Repository
{
    public class SqlDbTransactionWrapper : IDbTransactionWrapper
    {
        private readonly IDbContextTransaction _transaction;

        public SqlDbTransactionWrapper(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync() => _transaction.CommitAsync();
        public Task RollbackAsync() => _transaction.RollbackAsync();
    }

    public class SqlSalesRepository : ISalesRepository
    {
        private readonly SalesDbContext _context;

        public SqlSalesRepository(SalesDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task SaveCartAsync(Cart cart)
        {
            var exists = await _context.Carts.AnyAsync(c => c.UserId == cart.UserId);
            if (!exists)
            {
                _context.Carts.Add(cart);
            }
            else
            {
                _context.Carts.Update(cart);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SaveOrderReceiptAsync(OrderReceiptDto orderDto)
        {
            var order = new OrderReceipt(
                id: orderDto.Id,
                firstName: orderDto.FirstName,
                lastName: orderDto.LastName,
                storeId: orderDto.StoreId,
                extraPickups: orderDto.ExtraPickups != null ? string.Join(",", orderDto.ExtraPickups) : null,
                subTotal: orderDto.SubTotal,
                tax: orderDto.Tax,
                total: orderDto.Total,
                placedAt: orderDto.PlacedAt
            );

            foreach (var itemDto in orderDto.Items)
            {
                var item = new OrderReceiptItem(
                    sku: itemDto.VariantId,
                    productId: itemDto.ProductId,
                    productName: itemDto.ProductName,
                    variantName: itemDto.VariantName,
                    price: itemDto.Price,
                    quantity: itemDto.Quantity,
                    image: itemDto.Image
                );
                order.AddItem(item);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<OrderReceiptDto?> GetOrderByIdAsync(string id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            var itemsDto = order.Items.Select(i => new CartItemDto(
                ProductId: i.ProductId,
                VariantId: i.Sku,
                ProductName: i.ProductName,
                VariantName: i.VariantName,
                Price: i.Price,
                Quantity: i.Quantity,
                Image: i.Image
            )).ToList();

            return new OrderReceiptDto(
                Id: order.Id,
                FirstName: order.FirstName,
                LastName: order.LastName,
                StoreId: order.StoreId,
                ExtraPickups: order.ExtraPickups?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                Items: itemsDto,
                SubTotal: order.SubTotal,
                Tax: order.Tax,
                Total: order.Total,
                PlacedAt: order.PlacedAt
            );
        }

        public async Task<IDbTransactionWrapper> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return new SqlDbTransactionWrapper(transaction);
        }
    }
}
