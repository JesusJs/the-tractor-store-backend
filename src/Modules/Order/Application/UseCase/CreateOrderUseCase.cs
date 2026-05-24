using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Application.UseCase.TractorEcommerce.Modules.Order.Application.UseCases;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TractorEcommerce.Modules.Order.Application.UseCase
{

    namespace TractorEcommerce.Modules.Order.Application.UseCases
    {
        public class CreateOrderUseCase
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IEventBus _eventBus;

            public CreateOrderUseCase(IOrderRepository orderRepository, IEventBus eventBus)
            {
                _orderRepository = orderRepository;
                _eventBus = eventBus;
            }

            public async Task ExecuteAsync(OrderPayloadDto payload)
            {
                // 1. Generamos el ID único de la orden
                var orderId = Guid.NewGuid().ToString();

                // 2. Calculamos los valores financieros (Subtotal, Tax, Total)
                decimal subTotal = payload.Items.Sum(i => i.Price * i.Quantity);
                decimal tax = subTotal * 0.19m; // Simulación de IVA (ajusta el porcentaje si es necesario)
                decimal total = subTotal + tax;

                // 3. Mapeamos exactamente al OrderReceiptDto que espera tu repositorio real 🛠️
                var orderReceipt = new OrderReceiptDto(
                    Id: orderId,
                    FirstName: payload.FirstName,
                    LastName: payload.LastName,
                    StoreId: payload.StoreId,
                    ExtraPickups: payload.ExtraPickups,
                    Items: payload.Items.Select(i => new OrderItemDetailDto(
                        ProductId: i.ProductId,
                        VariantId: i.VariantId, // Tu SKU de inventario
                        ProductName: i.ProductName,
                        VariantName: i.VariantName,
                        Price: i.Price,
                        Quantity: i.Quantity,
                        Image: i.Image
                    )).ToList(),
                    SubTotal: subTotal,
                    Tax: tax,
                    Total: total,
                    PlacedAt: DateTime.UtcNow,
                    Status: "Pending"
                );

                // Guardamos en la base de datos (OrderDbContext) con tu método exacto 🛠️
                await _orderRepository.SaveOrderAsync(orderReceipt);

                // 4. Mapeamos y disparamos el evento a Kafka para reservar en Inventory 🚀
                // Usamos VariantId como el Sku en el contrato de integración
                var stockItems = payload.Items
                    .Select(i => new OrderStockItemDto(i.VariantId, i.Quantity))
                    .ToList();

                var integrationEvent = new OrderPlacedIntegrationEvent(
                    OrderId: Guid.Parse(orderId),
                    CustomerId: $"{payload.FirstName} {payload.LastName}",
                    Items: stockItems
                );

                // Publicamos al tópico que el módulo de Inventario estará escuchando
                await _eventBus.PublishAsync(
                    topic: "order.orders.placed",
                    key: orderId,
                    message: integrationEvent
                );
            }
        }
        public record OrderPlacedIntegrationEvent(
        Guid OrderId,
        string CustomerId,
        List<OrderStockItemDto> Items
    );

        public record OrderStockItemDto(
            string Sku,
            int Quantity
        );
    }
}
