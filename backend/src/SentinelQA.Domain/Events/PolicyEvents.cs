using System;

namespace SentinelQA.Domain.Events;

public sealed record PolicyCreated(
    Guid PolicyId,
    Guid TenantId,
    Guid FirewallId,
    string Name,
    int Version) : DomainEvent;

public sealed record PolicyUpdated(
    Guid PolicyId,
    Guid TenantId,
    int Version) : DomainEvent;