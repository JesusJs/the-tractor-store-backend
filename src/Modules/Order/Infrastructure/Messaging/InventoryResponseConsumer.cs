using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TractorEcommerce.Modules.Order.Application.Eventos;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Order.Infrastructure.Messaging
{
    public class InventoryResponseConsumer : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private const string SuccessTopic = "inventory.orders.reserved";
        private const string FailureTopic = "inventory.orders.failed";

        public InventoryResponseConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = "tractor-order-inventory-response-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                // Nos suscribimos a ambos tópicos (Éxito y Fallo) 🎧
                _consumer.Subscribe(new[] { SuccessTopic, FailureTopic });

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult == null) continue;

                        using var scope = _serviceProvider.CreateScope();
                        var orderRepo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                        // Determinamos qué tipo de evento llegó dependiendo del tópico
                        if (consumeResult.Topic == SuccessTopic)
                        {
                            var successEvent = JsonSerializer.Deserialize<InventoryReservedIntegrationEvent>(consumeResult.Message.Value, jsonOptions);
                            if (successEvent != null)
                            {
                                await ProcessOrderApprovalAsync(orderRepo, successEvent.OrderId.ToString());
                            }
                        }
                        else if (consumeResult.Topic == FailureTopic)
                        {
                            var failureEvent = JsonSerializer.Deserialize<InventoryReservationFailedIntegrationEvent>(consumeResult.Message.Value, jsonOptions);
                            if (failureEvent != null)
                            {
                                await ProcessOrderRejectionAsync(orderRepo, failureEvent.OrderId.ToString(), failureEvent.Reason);
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
                        Console.WriteLine($"Error en el consumidor de respuestas de órdenes: {ex.Message}");
                    }
                }

                _consumer.Close();
            }, stoppingToken);
        }

        private async Task ProcessOrderApprovalAsync(IOrderRepository orderRepo, string orderId)
        {
            // 1. Buscamos el DTO de la orden usando tu método real
            var order = await orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return;

            // 2. Transición de estado a Confirmada (como es un record, usamos 'with' para mutarlo de forma segura)
            var updatedOrder = order with { Status = "Confirmed" };

            // 3. Persistimos de vuelta en la base de datos
            await orderRepo.SaveOrderAsync(updatedOrder);
            Console.WriteLine($"[Order] ¡SAGA COMPLETA! Orden {orderId} marcada como CONFIRMADA por stock garantizado.");
        }

        private async Task ProcessOrderRejectionAsync(IOrderRepository orderRepo, string orderId, string reason)
        {
            var order = await orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return;

            // Transición de estado a Cancelada por falta de inventario
            // Si el DTO no maneja el campo de texto de error, el estado "CancelledByInventory" o similar ya ayuda muchísimo
            var updatedOrder = order with { Status = "CancelledByInventory" };

            await orderRepo.SaveOrderAsync(updatedOrder);
            Console.WriteLine($"[Order] ¡SAGA ABORTADA! Orden {orderId} CANCELADA. Motivo: {reason}");
        }
    }
}
