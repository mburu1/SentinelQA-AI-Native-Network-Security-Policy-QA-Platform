using MediatR;
using SentinelQA.Application.Abstractions;
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
    public async Task WriteAsync(Guid tenantId, string action, string entityType, string entityId, CancellationToken cancellationToken)
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

internal sealed class AuditPolicyCreated(AuditWriter writer) : INotificationHandler<PolicyCreated>
{
    public Task Handle(PolicyCreated notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(notification.TenantId, "PolicyCreated", "Policy", notification.PolicyId.ToString(), cancellationToken);
}

internal sealed class AuditChangeRequestSubmitted(AuditWriter writer) : INotificationHandler<ChangeRequestSubmitted>
{
    public Task Handle(ChangeRequestSubmitted notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(notification.TenantId, "ChangeRequestSubmitted", "ChangeRequest", notification.ChangeRequestId.ToString(), cancellationToken);
}

internal sealed class AuditChangeRequestApproved(AuditWriter writer) : INotificationHandler<ChangeRequestApproved>
{
    public Task Handle(ChangeRequestApproved notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(notification.TenantId, "ChangeRequestApproved", "ChangeRequest", notification.ChangeRequestId.ToString(), cancellationToken);
}

internal sealed class AuditDeploymentCompleted(AuditWriter writer) : INotificationHandler<DeploymentCompleted>
{
    public Task Handle(DeploymentCompleted notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(notification.TenantId, "DeploymentCompleted", "ChangeRequest", notification.ChangeRequestId.ToString(), cancellationToken);
}

internal sealed class AuditDefectCreated(AuditWriter writer) : INotificationHandler<DefectCreated>
{
    public Task Handle(DefectCreated notification, CancellationToken cancellationToken) =>
        writer.WriteAsync(notification.TenantId, "DefectCreated", "Defect", notification.DefectId.ToString(), cancellationToken);
}