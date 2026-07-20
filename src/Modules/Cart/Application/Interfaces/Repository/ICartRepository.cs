using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Domain.Entities;

namespace TractorEcommerce.Modules.Cart.Application.Interfaces.Repository
{
    public interface ICartRepository
    {
        Task<CartEntity?> GetByUserIdAsync(string userId);

        Task SaveAsync(CartEntity cart);
    }
}
