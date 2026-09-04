namespace SentinelQA.Domain.Events;

public sealed record ChangeRequestSubmitted(Guid ChangeRequestId, Guid TenantId, Guid PolicyId, Guid RequestedBy) : DomainEventBase;
public sealed record ChangeRequestApproved(Guid ChangeRequestId, Guid TenantId, Guid ApproverId) : DomainEventBase;
public sealed record ChangeRequestRejected(Guid ChangeRequestId, Guid TenantId, string Reason) : DomainEventBase;
public sealed record DeploymentStarted(Guid ChangeRequestId, Guid TenantId, Guid PolicyId) : DomainEventBase;
public sealed record DeploymentCompleted(Guid ChangeRequestId, Guid TenantId, bool Success, string? DeploymentRef) : DomainEventBase;