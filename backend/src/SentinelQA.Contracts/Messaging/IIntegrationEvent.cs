namespace SentinelQA.Contracts.Messaging;

/// <summary>A fact that has occurred, published to Kafka via the transactional outbox.</summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    string EventType { get; }
    DateTimeOffset OccurredOn { get; }
    Guid TenantId { get; }
    Guid CorrelationId { get; }
}