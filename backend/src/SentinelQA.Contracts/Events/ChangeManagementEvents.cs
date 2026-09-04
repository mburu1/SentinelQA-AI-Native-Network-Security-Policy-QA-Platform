using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Contracts.Events;

public sealed record ChangeRequestSubmittedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid ChangeRequestId, Guid PolicyId, Guid RequestedBy)
    : IntegrationEventBase(TenantId, CorrelationId);

public sealed record ChangeRequestApprovedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid ChangeRequestId, Guid ApproverId)
    : IntegrationEventBase(TenantId, CorrelationId);

public sealed record DeploymentCompletedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid ChangeRequestId, bool Success, string? DeploymentRef)
    : IntegrationEventBase(TenantId, CorrelationId);