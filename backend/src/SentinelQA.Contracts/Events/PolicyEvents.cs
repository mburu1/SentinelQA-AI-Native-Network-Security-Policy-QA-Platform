using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Contracts.Events;

public sealed record PolicyCreatedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid PolicyId, Guid FirewallId, string Name, int Version)
    : IntegrationEventBase(TenantId, CorrelationId);

public sealed record PolicyValidatedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid PolicyId, bool Passed, int FindingsCount, int CriticalCount)
    : IntegrationEventBase(TenantId, CorrelationId);