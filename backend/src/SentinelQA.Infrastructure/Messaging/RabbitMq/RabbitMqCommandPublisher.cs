using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Contracts.Messaging;
using SentinelQA.Infrastructure.Common;

namespace SentinelQA.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqCommandPublisher(RabbitMqConnectionProvider connections, ILogger<RabbitMqCommandPublisher> logger) : ICommandPublisher
{
    public async Task PublishAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        var connection = await connections.GetConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(RabbitMqRouting.Exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

        var routingKey = RabbitMqRouting.RoutingKeyFor(typeof(TCommand));
        var body = JsonSerializer.SerializeToUtf8Bytes(command, typeof(TCommand), JsonDefaults.Options);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = command.CommandId.ToString(),
            CorrelationId = command.CorrelationId.ToString(),
            Type = typeof(TCommand).AssemblyQualifiedName
        };

        await channel.BasicPublishAsync(RabbitMqRouting.Exchange, routingKey, mandatory: true, properties, body, cancellationToken);

        logger.LogInformation("Published command {CommandType} ({CommandId}) to RabbitMQ.", typeof(TCommand).Name, command.CommandId);
    }
}