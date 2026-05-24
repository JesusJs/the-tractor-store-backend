using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Cart.Application.Commands;

namespace TractorEcommerce.Modules.Cart.Application.UseCase
{

    public class AddToCartUseCase
    {
        // Cambiamos el puerto en memoria viejo por el contrato real que creamos hoy
        private readonly TractorEcommerce.Modules.Cart.Application.Interfaces.Repository.ICartRepository _cartRepository;

        public AddToCartUseCase(TractorEcommerce.Modules.Cart.Application.Interfaces.Repository.ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<TractorEcommerce.Modules.Cart.Domain.Entities.Cart> ExecuteAsync(AddToCartCommand command)
        {
            // 1. Obtiene el carrito real de Postgres
            var cart = await _cartRepository.GetByUserIdAsync(command.CartId);

            if (cart == null)
            {
                cart = new TractorEcommerce.Modules.Cart.Domain.Entities.Cart(command.CartId);
            }

            // 2. Aquí quemamos el precio de prueba o el que traigas del catálogo (ej. 150000) para ir rápido
            cart.AddItem(command.Sku, command.Quantity, command.Price);
            await _cartRepository.SaveAsync(cart);

            return cart;
        }
    }
}
