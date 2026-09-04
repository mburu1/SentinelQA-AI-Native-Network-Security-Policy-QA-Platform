namespace SentinelQA.Domain.Events;

public sealed record UserRegistered(Guid UserId, Guid TenantId, string Email) : DomainEventBase;