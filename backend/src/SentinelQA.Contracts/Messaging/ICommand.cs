namespace SentinelQA.Contracts.Messaging;

/// <summary>Work that must be performed, delivered through RabbitMQ.</summary>
public interface ICommand
{
    Guid CommandId { get; }
    Guid CorrelationId { get; }
    DateTimeOffset CreatedAt { get; }
}