using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Events;

namespace SentinelQA.Modules.Audit.EventHandlers;

internal sealed class AuditWriter(
    IRepository<AuditEntry, Guid> auditEntries,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ICorrelationContext correlation)
{
    public async Task WriteAsync(
        Guid tenantId,
        string action,
        string entityType,
        string entityId,
        CancellationToken cancellationToken)
    {
        var entry = AuditEntry.Record(
            tenantId,
            currentUser.IsAuthenticated ? currentUser.UserId : null,
            action,
            entityType,
            entityId,
            correlation.CorrelationId);

        await auditEntries.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class AuditPolicyCreated(AuditWriter writer)
    : INotificationHandler<DomainEventNotification<PolicyCreated>>
{
    public Task Handle(DomainEventNotification<PolicyCreated> notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(
            notification.DomainEvent.TenantId,
            "PolicyCreated",
            "Policy",
            notification.DomainEvent.PolicyId.ToString(),
            cancellationToken);
}

internal sealed class AuditChangeRequestSubmitted(AuditWriter writer)
    : INotificationHandler<DomainEventNotification<ChangeRequestSubmitted>>
{
    public Task Handle(DomainEventNotification<ChangeRequestSubmitted> notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(
            notification.DomainEvent.TenantId,
            "ChangeRequestSubmitted",
            "ChangeRequest",
            notification.DomainEvent.ChangeRequestId.ToString(),
            cancellationToken);
}

internal sealed class AuditChangeRequestApproved(AuditWriter writer)
    : INotificationHandler<DomainEventNotification<ChangeRequestApproved>>
{
    public Task Handle(DomainEventNotification<ChangeRequestApproved> notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(
            notification.DomainEvent.TenantId,
            "ChangeRequestApproved",
            "ChangeRequest",
            notification.DomainEvent.ChangeRequestId.ToString(),
            cancellationToken);
}

internal sealed class AuditDeploymentCompleted(AuditWriter writer)
    : INotificationHandler<DomainEventNotification<DeploymentCompleted>>
{
    public Task Handle(DomainEventNotification<DeploymentCompleted> notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(
            notification.DomainEvent.TenantId,
            "DeploymentCompleted",
            "ChangeRequest",
            notification.DomainEvent.ChangeRequestId.ToString(),
            cancellationToken);
}

internal sealed class AuditDefectCreated(AuditWriter writer)
    : INotificationHandler<DomainEventNotification<DefectCreated>>
{
    public Task Handle(DomainEventNotification<DefectCreated> notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(
            notification.DomainEvent.TenantId,
            "DefectCreated",
            "Defect",
            notification.DomainEvent.DefectId.ToString(),
            cancellationToken);
}