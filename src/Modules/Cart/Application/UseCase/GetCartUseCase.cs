using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Cart.Application.UseCase
{
    public class GetCartUseCase
    {
        private readonly ICartRepository _cartRepository;

        public GetCartUseCase(ICartRepository cartRepository) { _cartRepository = cartRepository; }

        public async Task<TractorEcommerce.Modules.Cart.Domain.Entities.Cart> ExecuteAsync(string userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            return cart ?? new Domain.Entities.Cart(userId); // Si no existe, devuelve uno vacío listo para el Front
        }
    }
}
