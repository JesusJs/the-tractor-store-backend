using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Domain.Entities;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Order.Application.UseCase
{
    public class CreateOrderFromCheckoutUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateOrderFromCheckoutUseCase> _logger;

        public CreateOrderFromCheckoutUseCase(
            IOrderRepository orderRepository,
            IEventBus eventBus,
            ILogger<CreateOrderFromCheckoutUseCase> logger)
        {
            _orderRepository = orderRepository;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task HandleAsync(CheckoutRequestedEvent @event)
        {
            _logger.LogInformation("Consumiendo CheckoutRequestedEvent. Creando orden para el cliente: {CustomerId}", @event.CustomerId);

            try
            {
                // 1. Mapear los DTOs del evento a entidades de nuestro dominio de Order
                var orderItems = @event.Items.Select(item =>
                    new OrderLineItem(Guid.NewGuid(), item.Sku, item.Quantity, item.Price)
                ).ToList();

                var order = new CustomerOrder(Guid.NewGuid(), @event.CustomerId, orderItems);

                // 2. Persistir en la base de datos independiente del módulo Order
                await _orderRepository.SaveAsync(order);
                _logger.LogInformation("Orden guardada exitosamente en la base de datos con ID: {OrderId}", order.Id);

                // 3. Crear el evento definitivo de integración en pasado
                var orderPlacedEvent = new OrderPlacedEvent(
                    OrderId: order.Id,
                    CustomerId: order.CustomerId,
                    Items: @event.Items, // Reutilizamos la lista de DTOs compartidos
                    PlacedAt: order.CreatedAt
                );

                // 4. Publicar a Kafka usando la firma de 3 parámetros. La key será el OrderId en string.
                await _eventBus.PublishAsync("order-placed-topic", order.Id.ToString(), orderPlacedEvent);

                _logger.LogInformation("OrderPlacedEvent publicado con éxito en Kafka para la orden: {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al procesar la creación de la orden para el cliente {CustomerId}", @event.CustomerId);
                throw;
            }
        }
    }
}
