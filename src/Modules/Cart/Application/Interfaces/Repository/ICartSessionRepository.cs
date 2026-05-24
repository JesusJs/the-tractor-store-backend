using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Domain.Entities;

namespace TractorEcommerce.Modules.Cart.Application.Interfaces.Repository
{
    public interface ICartSessionRepository
    {
        Task<ShoppingCart> GetCartAsync(string cartId);
        Task SaveCartAsync(ShoppingCart cart);
    }
}
