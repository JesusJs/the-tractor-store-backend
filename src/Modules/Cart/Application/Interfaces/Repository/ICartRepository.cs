using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Cart.Application.Interfaces.Repository
{
    public interface ICartRepository
    {
        Task<TractorEcommerce.Modules.Cart.Domain.Entities.Cart?> GetByUserIdAsync(string userId);

        Task SaveAsync(TractorEcommerce.Modules.Cart.Domain.Entities.Cart cart);
    }
}
