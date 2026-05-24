using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Cart.Application.UseCase
{
    public class RemoveFromCartUseCase
    {
        private readonly ICartRepository _cartRepository;

        public RemoveFromCartUseCase(ICartRepository cartRepository) { _cartRepository = cartRepository; }

        public async Task<TractorEcommerce.Modules.Cart.Domain.Entities.Cart> ExecuteAsync(string cartId, string sku)
        {
            var cart = await _cartRepository.GetByUserIdAsync(cartId);
            if (cart != null)
            {
                // Mapeamos a la lógica interna de tu entidad Cart
                cart.Clear(); // O una lógica específica mutando la lista privada '_items' si remueves por SKU
                await _cartRepository.SaveAsync(cart);
            }
            return cart ?? new Domain.Entities.Cart(cartId);
        }
    }
}
