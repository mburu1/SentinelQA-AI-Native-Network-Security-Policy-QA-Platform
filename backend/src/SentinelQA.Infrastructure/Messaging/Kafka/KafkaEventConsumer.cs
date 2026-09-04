using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SentinelQA.Infrastructure.Messaging.Kafka;

/// <summary>
/// Baseline consumer proving the event stream is consumable.
/// Downstream projections (analytics, audit replication, AI enrichment) plug in here.
/// </summary>
public sealed class KafkaEventConsumer(
    IOptions<KafkaOptions> options,
    IOptions<MessagingOptions> messagingOptions,
    ILogger<KafkaEventConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!messagingOptions.Value.Enabled)
        {
            logger.LogInformation("Messaging disabled. Kafka consumer will not start.");
            return;
        }

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = "sentinelqa-backend",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        }).Build();

        consumer.Subscribe(KafkaTopics.All);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                logger.LogDebug("Kafka event received on {Topic}: {Key}", result.Topic, result.Message.Key);
            }
        }
        catch (OperationCanceledException)
        {
            // graceful shutdown
        }
        finally
        {
            consumer.Close();
        }

        await Task.CompletedTask;
    }
}