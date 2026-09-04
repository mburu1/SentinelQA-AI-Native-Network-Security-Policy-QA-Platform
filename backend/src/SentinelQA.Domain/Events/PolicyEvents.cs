namespace SentinelQA.Domain.Events;

public sealed record PolicyCreated(Guid PolicyId, Guid TenantId, Guid FirewallId, string Name, int Version) : DomainEventBase;
public sealed record PolicyUpdated(Guid PolicyId, Guid TenantId, int Version) : DomainEventBase;
public sealed record PolicyValidated(Guid PolicyId, Guid TenantId, bool Passed, int FindingsCount, int CriticalCount) : DomainEventBase;