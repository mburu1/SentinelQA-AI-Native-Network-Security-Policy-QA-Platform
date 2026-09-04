namespace SentinelQA.Domain.Events;

public sealed record PolicyCreatedEvent(
    Guid PolicyId,
    Guid FirewallId,
    DateTimeOffset CreatedAt);
