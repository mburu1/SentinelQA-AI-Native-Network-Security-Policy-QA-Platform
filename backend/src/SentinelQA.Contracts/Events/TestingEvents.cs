using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Contracts.Events;

public sealed record TestRunCompletedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid TestRunId, string Status, int Total, int Passed, int Failed)
    : IntegrationEventBase(TenantId, CorrelationId);

public sealed record DefectCreatedIntegrationEvent(Guid TenantId, Guid CorrelationId, Guid DefectId, string Title, string Severity)
    : IntegrationEventBase(TenantId, CorrelationId);