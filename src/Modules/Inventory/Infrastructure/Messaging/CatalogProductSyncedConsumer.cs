using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Inventory.Infrastructure.Messaging
{
    public class CatalogProductSyncedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CatalogProductSyncedConsumer> _logger;
        private readonly string _topic = "catalog.products.stock-updated"; // Tópico que publica el Catálogo

        public CatalogProductSyncedConsumer(IServiceScopeFactory scopeFactory, ILogger<CatalogProductSyncedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);

        private async Task StartConsumerLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "tractor-inventory-sync-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null) continue;

                    var catalogEvent = JsonSerializer.Deserialize<ProductStockStockPayload>(
                        consumeResult.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (catalogEvent != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var inventoryRepo = scope.ServiceProvider.GetRequiredService<IInventoryRepository>();

                        // Sincronizamos de forma autónoma el stock en la base de datos de Inventario
                        await inventoryRepo.UpsertStockAsync(catalogEvent.Sku, catalogEvent.NewStock);

                        _logger.LogInformation("Inventario Sincronizado: SKU {Sku} actualizado a {Stock} unidades.", catalogEvent.Sku, catalogEvent.NewStock);
                    }

                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sincronizando inventario desde Catálogo: {Message}", ex.Message);
                }
            }
            consumer.Close();
        }
    }

    public record ProductStockStockPayload(string Sku, int NewStock);
}
