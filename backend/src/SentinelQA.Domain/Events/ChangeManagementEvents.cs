using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Events;

public sealed record ChangeRequestSubmitted(
    Guid ChangeRequestId,
    Guid TenantId,
    Guid PolicyId,
    Guid RequestedBy) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record ChangeRequestApproved(
    Guid ChangeRequestId,
    Guid TenantId,
    Guid ApproverId) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record ChangeRequestRejected(
    Guid ChangeRequestId,
    Guid TenantId,
    string Reason) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record DeploymentStarted(
    Guid ChangeRequestId,
    Guid TenantId,
    Guid PolicyId) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record DeploymentCompleted(
    Guid ChangeRequestId,
    Guid TenantId,
    bool Success,
    string? DeploymentRef) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}