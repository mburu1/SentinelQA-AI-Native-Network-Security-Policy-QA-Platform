using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Infrastructure.Common;

namespace SentinelQA.Infrastructure.Messaging.RabbitMq;

/// <summary>
/// Consumes commands from RabbitMQ and dispatches them to registered ICommandHandler implementations.
/// Runs as a hosted service inside the modular monolith host (extractable to a dedicated worker later).
/// </summary>
public sealed class RabbitMqCommandConsumer(
    RabbitMqConnectionProvider connections,
    IServiceScopeFactory scopeFactory,
    IOptions<MessagingOptions> messagingOptions,
    ILogger<RabbitMqCommandConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!messagingOptions.Value.Enabled)
        {
            logger.LogInformation("Messaging disabled. RabbitMQ consumer will not start.");
            return;
        }

        var connection = await connections.GetConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync(RabbitMqRouting.Exchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        foreach (var queue in RabbitMqRouting.AllQueues)
        {
            var arguments = new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = $"{RabbitMqRouting.Exchange}.dlx"
            };

            await channel.ExchangeDeclareAsync($"{RabbitMqRouting.Exchange}.dlx", ExchangeType.Fanout, durable: true, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments, cancellationToken: stoppingToken);
            await channel.QueueBindAsync(queue, RabbitMqRouting.Exchange, queue, cancellationToken: stoppingToken);
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await DispatchAsync(args, stoppingToken);
                await channel.BasicAckAsync(args.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Command consumption failed for routing key {RoutingKey}. Sending to DLQ.", args.RoutingKey);
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        foreach (var queue in RabbitMqRouting.AllQueues)
            await channel.BasicConsumeAsync(queue, autoAck: false, consumer, stoppingToken);

        logger.LogInformation("RabbitMQ command consumer started for queues: {Queues}", string.Join(", ", RabbitMqRouting.AllQueues));

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DispatchAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        var commandType = Type.GetType(args.BasicProperties.Type)
            ?? throw new InvalidOperationException($"Unknown command type '{args.BasicProperties.Type}'.");

        var command = JsonSerializer.Deserialize(args.Body.Span, commandType, JsonDefaults.Options)
            ?? throw new InvalidOperationException("Failed to deserialize command payload.");

        await using var scope = scopeFactory.CreateAsyncScope();

        var handler = scope.ServiceProvider
            .GetServices<ICommandHandler>()
            .FirstOrDefault(h => h.CommandType == commandType)
            ?? throw new InvalidOperationException($"No handler registered for command '{commandType.Name}'.");

        using var _ = Persistence.Outbox.AsyncLocalCorrelationContext.Begin(Guid.Parse(args.BasicProperties.CorrelationId));

        await handler.HandleAsync(command, cancellationToken);
    }
}