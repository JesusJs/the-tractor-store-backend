using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Messaging;

public class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventBus(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            AllowAutoCreateTopics = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, string key, T message) where T : class
    {
        var jsonPayload = JsonSerializer.Serialize(message);
        
        var kafkaMessage = new Message<string, string>
        {
            Key = key,
            Value = jsonPayload
        };

        // SOLID: No bloqueamos el hilo principal, manejamos la entrega asíncrona
        await _producer.ProduceAsync(topic, kafkaMessage);
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}