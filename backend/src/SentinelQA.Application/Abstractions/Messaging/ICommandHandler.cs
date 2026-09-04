using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Application.Abstractions.Messaging;

public interface ICommandHandler
{
    Type CommandType { get; }
    Task HandleAsync(object command, CancellationToken cancellationToken);
}

public abstract class CommandHandlerBase<TCommand> : ICommandHandler
    where TCommand : class, ICommand
{
    public Type CommandType => typeof(TCommand);

    public abstract Task HandleAsync(TCommand command, CancellationToken cancellationToken);

    Task ICommandHandler.HandleAsync(object command, CancellationToken cancellationToken) =>
        HandleAsync((TCommand)command, cancellationToken);
}