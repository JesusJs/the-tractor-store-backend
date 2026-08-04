using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Cart.Domain.Entities;

namespace TractorEcommerce.Modules.Cart.Application.UseCase
{
    public class GetCartUseCase
    {
        private readonly ICartRepository _cartRepository;

        public GetCartUseCase(ICartRepository cartRepository) { _cartRepository = cartRepository; }

        public async Task<CartEntity> ExecuteAsync(string userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            return cart ?? new CartEntity(userId); 
        }
    }
}
