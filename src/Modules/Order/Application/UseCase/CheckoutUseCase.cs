using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Domain.Entities;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Order.Application.UseCase
{
    public class CheckoutUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;

        public CheckoutUseCase(IOrderRepository orderRepository, IEventBus eventBus)
        {
            _orderRepository = orderRepository;
            _eventBus = eventBus;
        }

        public async Task<OrderReceiptDto> ExecuteAsync(string userId, OrderPayloadDto payload)
        {
            if (payload.Items == null || !payload.Items.Any())
                throw new InvalidOperationException("No se puede procesar un pedido sin artículos.");

            // Generar ID único de pedido para la página de Thanks
            string orderId = $"ORD-{new Random().Next(100000, 999999)}";

            // Mapear ítems y procesar cálculos inmutables
            var orderItems = payload.Items.Select(i => new OrderItemDetailDto(
                i.ProductId, i.VariantId, i.ProductName, i.VariantName, i.Price, i.Quantity, i.Image
            )).ToList();

            decimal subTotal = orderItems.Sum(i => i.Price * i.Quantity);
            decimal tax = subTotal * 0.19m; // IVA Colombia
            decimal total = subTotal + tax;

            var receipt = new OrderReceiptDto(
                Id: orderId,
                FirstName: payload.FirstName,
                LastName: payload.LastName,
                StoreId: payload.StoreId,
                ExtraPickups: payload.ExtraPickups,
                Items: orderItems,
                SubTotal: subTotal,
                Tax: tax,
                Total: total,
                PlacedAt: DateTime.UtcNow,
                Status: "Pending"
            );

            // 1. Guardar de forma aislada en el esquema 'ordering'
            await _orderRepository.SaveOrderAsync(receipt);

            // 2. Notificación Asíncrona vía Kafka
            var kafkaPayload = new OrderPlacedKafkaEvent(
                OrderId: orderId,
                UserId: userId,
                Items: orderItems.Select(i => new KafkaItem(i.VariantId, i.Quantity)).ToList()
            );

            await _eventBus.PublishAsync(
                topic: "sales.orders.placed",
                key: orderId,
                message: kafkaPayload
            );

            return receipt;
        }
    }

    // Estructuras serializables para el tópico de Kafka
    public record OrderPlacedKafkaEvent(string OrderId, string UserId, List<KafkaItem> Items);
    public record KafkaItem(string VariantId, int Quantity);
}
