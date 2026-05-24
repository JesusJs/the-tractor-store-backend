using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TractorEcommerce.Modules.Order.Infrastructure.Messaging
{
    public class OrderPlacedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly string _topic = "sales.orders.placed";
        private readonly string _bootstrapServers = "localhost:9092"; // Cambiar por tu config de appsettings

        public OrderPlacedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderPlacedConsumer> _logger)
        {
            _scopeFactory = scopeFactory;
            _logger = _logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ejecutamos el loop de escucha en un hilo secundario para no bloquear el arranque de la API
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "tractor-cart-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false // Controlamos el commit manualmente para evitar pérdida de datos
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            _logger.LogInformation("Subscrito al tópico de Kafka: {Topic}. Esperando eventos...", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult == null) continue;

                    _logger.LogInformation("Evento recibido desde Kafka. Procesando limpieza de carrito para la orden: {Key}", consumeResult.Message.Key);

                    // Deserializamos el evento inyectando dinámicamente el modelo
                    var orderEvent = JsonSerializer.Deserialize<OrderPlacedEventData>(consumeResult.Message.Value);

                    if (orderEvent != null && !string.IsNullOrWhiteSpace(orderEvent.UserId))
                    {
                        // Abrimos un Scope temporal porque el Repositorio (EF Core) es Scoped y este servicio es Singleton
                        using var scope = _scopeFactory.CreateScope();
                        var cartRepository = scope.ServiceProvider.GetRequiredService<ICartRepository>();

                        var cart = await cartRepository.GetByUserIdAsync(orderEvent.UserId);
                        if (cart != null)
                        {
                            cart.Clear();
                            await cartRepository.SaveAsync(cart);
                            _logger.LogInformation("Carrito del usuario {UserId} vaciado de forma autónoma con éxito.", orderEvent.UserId);
                        }
                    }

                    // Confirmar a Kafka que el mensaje fue procesado correctamente
                    consumer.Commit(consumeResult);
                }
                catch (ConsumeException e)
                {
                    _logger.LogError("Error consumiendo mensaje de Kafka: {Error}", e.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error inesperado en el flujo del consumidor: {Message}", ex.Message);
                }
            }

            consumer.Close();
        }
    }
}
