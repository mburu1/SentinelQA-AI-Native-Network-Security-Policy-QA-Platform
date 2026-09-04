using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Events;

public abstract record DomainEventBase : IDomainEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}