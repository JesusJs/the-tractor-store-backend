using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Cart.Application.UseCase
{
    public class ClearCartUseCase
    {
        private readonly ICartRepository _cartRepository;

        public ClearCartUseCase(ICartRepository cartRepository) { _cartRepository = cartRepository; }

        public async Task ExecuteAsync(string userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart != null)
            {
                cart.Clear();
                await _cartRepository.SaveAsync(cart);
            }
        }
    }
}
