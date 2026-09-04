namespace SentinelQA.Contracts.Messaging;

public abstract record IntegrationEventBase(Guid TenantId, Guid CorrelationId) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public virtual string EventType => GetType().Name;
}