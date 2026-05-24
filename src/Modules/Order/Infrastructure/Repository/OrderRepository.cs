using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Infrastructure.Data;

namespace TractorEcommerce.Modules.Order.Infrastructure.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task SaveOrderAsync(OrderReceiptDto order)
        {
            // Aquí realizas la inserción en las tablas del esquema 'ordering'
            // Puedes mapear el DTO a tus entidades físicas de Orden si manejas DDD estricto
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task<OrderReceiptDto?> GetOrderByIdAsync(string orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
