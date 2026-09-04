using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Application.Abstractions.Messaging;

/// <summary>Publishes commands to RabbitMQ ("what work should happen").</summary>
public interface ICommandPublisher
{
    Task PublishAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand;
}