using Confluent.Kafka;
using System.Text.Json;

namespace AtlasBank.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
}

public class KafkaProducer : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            EnableIdempotence = true,
            Acks = Acks.All,
            MaxInFlight = 5,
            MessageSendMaxRetries = 10
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = json,
            Timestamp = Timestamp.Default
        };

        await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}
