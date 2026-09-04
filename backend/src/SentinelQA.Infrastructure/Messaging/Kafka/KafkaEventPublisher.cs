using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SentinelQA.Contracts.Messaging;
using SentinelQA.Infrastructure.Common;

namespace SentinelQA.Infrastructure.Messaging.Kafka;

public sealed class KafkaEventPublisher : IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IOptions<KafkaOptions> options, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.Leader,
            EnableIdempotence = true
        }).Build();
    }

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var topic = KafkaTopics.For(integrationEvent.EventType);
        var payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonDefaults.Options);

        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = integrationEvent.TenantId.ToString(),
            Value = payload
        }, cancellationToken);

        _logger.LogInformation("Published {EventType} to Kafka topic {Topic}.", integrationEvent.EventType, topic);
    }

    public ValueTask DisposeAsync()
    {
        _producer.Dispose();
        return ValueTask.CompletedTask;
    }
}