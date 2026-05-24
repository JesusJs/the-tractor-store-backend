using Microsoft.Extensions.Logging;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Cart.Application.UseCase
{
    public class CheckoutUseCase
    {
        private readonly ICartSessionRepository _cartRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CheckoutUseCase> _logger;

        public CheckoutUseCase(
            ICartSessionRepository cartRepository,
            IEventBus eventBus,
            ILogger<CheckoutUseCase> logger)
        {
            _cartRepository = cartRepository;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task ExecuteAsync(string cartId, string customerId)
        {
            _logger.LogInformation("Procesando Checkout para el carrito con SessionId: {CartId}", cartId);

            var cart = await _cartRepository.GetCartAsync(cartId);

            if (cart == null || !cart.Items.Any())
            {
                _logger.LogWarning("Intento de Checkout fallido: El carrito {CartId} está vacío.", cartId);
                throw new InvalidOperationException("No se puede hacer checkout de un carrito vacío.");
            }

            // Mapeamos los ítems del carrito local a los DTOs globales que viajan por el bus
            var itemsDto = cart.Items.Select(i => new CartItemDto(i.Sku, i.Quantity, i.Price)).ToList();

            // Creamos el evento de integración (en pasado, representando un hecho)
            var checkoutEvent = new CheckoutRequestedEvent(
                CartId: cart.Id,
                CustomerId: customerId,
                Items: itemsDto,
                TotalAmount: cart.TotalAmount
            );

            await _eventBus.PublishAsync("checkout-requested-topic", cart.Id, checkoutEvent);

            _logger.LogInformation("CheckoutRequestedEvent publicado exitosamente en Kafka para el carrito: {CartId}", cartId);
        }
    }
}
