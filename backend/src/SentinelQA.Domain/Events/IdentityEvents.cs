using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Events;

public sealed record UserRegistered(
    Guid UserId,
    Guid TenantId,
    string Email) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}