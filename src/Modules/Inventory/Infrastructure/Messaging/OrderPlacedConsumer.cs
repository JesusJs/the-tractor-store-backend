using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TractorEcommerce.Modules.Inventory.Application.Events;
using TractorEcommerce.Modules.Inventory.Application.IntegrationEvents;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Inventory.Infrastructure.Messaging
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventBus _eventBus;
        private const string TopicName = "order.orders.placed";

        public OrderPlacedConsumer(IConfiguration configuration, IServiceProvider serviceProvider, IEventBus eventBus)
        {
            _serviceProvider = serviceProvider;
            _eventBus = eventBus;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "tractor-inventory-order-processor-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false // Commiteamos manualmente al procesar todo con éxito
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _consumer.Subscribe(TopicName);

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult == null) continue;

                        var orderEvent = JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(consumeResult.Message.Value, jsonOptions);

                        if (orderEvent != null)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

                            bool allItemsReserved = true;
                            string failureReason = string.Empty;

                            // 1. PRIMERA PASADA: Validamos si todos los ítems tienen stock suficiente
                            // Hacemos esto antes de mutar para no dejar el inventario en un estado parcial si algo falla
                            foreach (var item in orderEvent.Items)
                            {
                                var inventoryItem = await inventoryRepo.GetBySkuAsync(item.Sku);

                                if (inventoryItem == null)
                                {
                                    allItemsReserved = false;
                                    failureReason = $"El producto con SKU {item.Sku} no existe en el inventario físico.";
                                    break;
                                }

                                // 🛠️ CORREGIDO: Usamos tu propiedad real 'AvailableStock'
                                if (inventoryItem.AvailableStock < item.Quantity)
                                {
                                    allItemsReserved = false;
                                    failureReason = $"Stock insuficiente para SKU {item.Sku}. Requerido: {item.Quantity}, Disponible: {inventoryItem.AvailableStock}.";
                                    break;
                                }
                            }

                            // 2. SEGUNDA PASADA: Si todo está OK, aplicamos los descuentos
                            if (allItemsReserved)
                            {
                                foreach (var item in orderEvent.Items)
                                {
                                    var inventoryItem = await inventoryRepo.GetBySkuAsync(item.Sku);

                                    // 1. Descontamos
                                    inventoryItem.DeductStock(item.Quantity);

                                    // 2. Persistimos en BD
                                    await inventoryRepo.UpdateAsync(inventoryItem);

                                    // 3. 🚀 ¡AQUÍ! Publicamos hacia Catálogo para que sincronice su Read-Model
                                    var stockChangedEvent = new ProductStockUpdatedEvent(
                                        Sku: inventoryItem.Sku,
                                        NewStock: inventoryItem.AvailableStock
                                    );

                                    await _eventBus.PublishAsync(
                                        topic: "inventory.stock.updated",
                                        key: inventoryItem.Sku,
                                        message: stockChangedEvent
                                    );
                                }

                                // 4. Finalmente, confirmamos la orden completa
                                var successEvent = new InventoryReservedIntegrationEvent(orderEvent.OrderId, DateTime.UtcNow);
                                await _eventBus.PublishAsync("inventory.orders.reserved", orderEvent.OrderId.ToString(), successEvent);
                            }
                            else
                            {
                                // Publicamos fallo (Transacción Compensatoria)
                                var failEvent = new InventoryReservationFailedIntegrationEvent(orderEvent.OrderId, failureReason, DateTime.UtcNow);
                                await _eventBus.PublishAsync("inventory.orders.failed", orderEvent.OrderId.ToString(), failEvent);
                                Console.WriteLine($"[Inventory] RESERVA FALLIDA para la Orden: {orderEvent.OrderId}. Motivo: {failureReason}");
                            }
                        }

                        _consumer.Commit(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error procesando evento de inventario: {ex.Message}");
                    }
                }

                _consumer.Close();
            }, stoppingToken);
        }
    }
}

