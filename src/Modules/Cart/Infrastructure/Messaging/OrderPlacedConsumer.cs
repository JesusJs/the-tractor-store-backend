using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Cart.Infrastructure.Messaging
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly string _topic = "sales.orders.placed";
        private readonly string _bootstrapServers = "localhost:9092";

        public OrderPlacedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderPlacedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "tractor-cart-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            _logger.LogInformation("Módulo Carrito: Subscrito a Kafka en {Topic}.", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null) continue;

                    // Deserializamos usando el DTO de mapeo
                    var orderEvent = JsonSerializer.Deserialize<OrderPlacedEventData>(consumeResult.Message.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (orderEvent != null && !string.IsNullOrWhiteSpace(orderEvent.UserId))
                    {
                        using var scope = _scopeFactory.CreateScope();

                        // Resolvemos el repositorio local del Carrito de forma segura
                        var cartRepository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

                        var cart = await cartRepository.GetByUserIdAsync(orderEvent.UserId);
                        if (cart != null)
                        {
                            cart.Clear();
                            await cartRepository.SaveAsync(cart);
                            _logger.LogInformation("Módulo Carrito: El carrito de {UserId} fue limpiado tras la orden {OrderId}", orderEvent.UserId, orderEvent.OrderId);
                        }
                    }

                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error en el consumidor del Carrito: {Message}", ex.Message);
                }
            }
            consumer.Close();
        }
    }
}
