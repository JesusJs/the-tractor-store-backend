using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Sales.Domain.Events;
using TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Events.Messaging
{
    public class KafkaOrderConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsumer<string, string> _consumer;

        public KafkaOrderConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "catalog-inventory-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Nos suscribimos al tópico de órdenes creadas emitido por el módulo de Sales
            _consumer.Subscribe("sales.orders.placed");

            // Ejecutamos el ciclo de consumo en un hilo dedicado de fondo para no bloquear la API
            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult == null) continue;

                        var orderEvent = JsonSerializer.Deserialize<OrderPlacedEvent>(consumeResult.Message.Value);

                        if (orderEvent != null)
                        {
                            // SOLID: Como un BackgroundService es Singleton, creamos un Scope 
                            // temporal para poder invocar al repositorio de base de datos (que es Scoped)
                            using var scope = _serviceProvider.CreateScope();
                            var catalogRepo = scope.ServiceProvider.GetRequiredService<ICatalogRepository>();

                            foreach (var item in orderEvent.Items)
                            {
                                var variant = await catalogRepo.GetVariantBySkuAsync(item.Sku);
                                if (variant != null)
                                {
                                    // Aplicamos la regla DDD: El agregado del dominio muta su propio estado
                                    variant.UpdateStock(variant.Stock - item.Quantity);
                                }
                            }

                            // Aquí es donde harías el await de tu DbContext para persistir en Postgres
                            Console.WriteLine($"[Kafka] Stock sincronizado con éxito en Catálogo para la orden: {orderEvent.OrderId}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Previene excepciones feas cuando el contenedor de Docker o la app se detiene
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Kafka Error] Error procesando evento de inventario: {ex.Message}");
                        // Requisito de robustez: Esperar un momento antes de reintentar si se cae el broker
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
            }, stoppingToken);
        }

        public override void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
