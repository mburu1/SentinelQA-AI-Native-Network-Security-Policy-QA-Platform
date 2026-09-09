using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SentinelQA.Contracts.Messaging;
using SentinelQA.Infrastructure.Common;
using SentinelQA.Infrastructure.Messaging.Kafka;
using SentinelQA.Infrastructure.Messaging;
//using SentinelQA.Infrastructure.Configuration;

namespace SentinelQA.Infrastructure.Persistence.Outbox;

/// <summary>
/// Polls the transactional outbox and publishes pending integration events to Kafka.
/// At-least-once delivery; consumers must be idempotent (EventId is stable).
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    KafkaEventPublisher kafka,
    IOptions<MessagingOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private const int MaxRetries = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Messaging disabled. Outbox processor will not start.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processing cycle failed.");
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var pending = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOn)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                var type = IntegrationEventRegistry.Resolve(message.EventType)
                    ?? throw new InvalidOperationException($"Unknown integration event type '{message.EventType}'.");

                var integrationEvent = (IIntegrationEvent)JsonSerializer.Deserialize(message.Payload, type, JsonDefaults.Options)!;

                await kafka.PublishAsync(integrationEvent, cancellationToken);

                message.ProcessedAt = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.RetryCount++;
                message.Error = ex.Message;
                logger.LogWarning(ex, "Failed to publish outbox message {MessageId} (attempt {Attempt}).", message.Id, message.RetryCount);
            }
        }

        if (pending.Count > 0)
            await context.SaveChangesAsync(cancellationToken);
    }
}