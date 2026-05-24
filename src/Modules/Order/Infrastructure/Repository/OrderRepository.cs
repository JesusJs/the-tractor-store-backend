using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Domain.Entities;
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

        public async Task SaveAsync(CustomerOrder order)
        {
            // EF Core detectará si es una nueva entidad o una actualización si ya está trackeada,
            // pero como usamos Add, forzamos la inserción de la orden raíz junto con sus ítems hijos.
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
    }
}
