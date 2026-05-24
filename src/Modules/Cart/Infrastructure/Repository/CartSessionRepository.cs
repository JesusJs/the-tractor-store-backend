using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Cart.Infrastructure.Data;

namespace TractorEcommerce.Modules.Cart.Infrastructure.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDbContext _context;

        public CartRepository(CartDbContext context)
        {
            _context = context;
        }

        // Usamos la ruta explícita en el tipo de retorno
        public async Task<TractorEcommerce.Modules.Cart.Domain.Entities.Cart?> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // Usamos la ruta explícita en el parámetro
        public async Task SaveAsync(TractorEcommerce.Modules.Cart.Domain.Entities.Cart cart)
        {
            var exists = await _context.Carts.AnyAsync(c => c.UserId == cart.UserId);

            if (!exists)
            {
                await _context.Carts.AddAsync(cart);
            }
            else
            {
                _context.Carts.Update(cart);
            }

            await _context.SaveChangesAsync();
        }
    }
}
