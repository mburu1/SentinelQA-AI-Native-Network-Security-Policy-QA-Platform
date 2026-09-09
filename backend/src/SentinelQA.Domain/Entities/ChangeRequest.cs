using System;
using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;

namespace SentinelQA.Domain.Entities;

public sealed class ChangeRequest : AggregateRoot<Guid> // Changed from Entity<Guid> to AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid PolicyId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid? ApproverId { get; private set; }
    public Guid? TestRunId { get; private set; }
    public ChangeRequestStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? DeploymentRef { get; private set; }

    private ChangeRequest() { } // EF Core

    public ChangeRequest(Guid tenantId, Guid policyId, Guid requestedBy, Guid? testRunId = null)
    {
        Id = Guid.CreateVersion7();
        TenantId = tenantId;
        PolicyId = policyId;
        RequestedBy = requestedBy;
        TestRunId = testRunId;
        Status = ChangeRequestStatus.Submitted;

        // Assuming AggregateRoot<T> exposes a method like RaiseDomainEvent or AddDomainEvent
        // AddDomainEvent(new ChangeRequestSubmitted(Id, TenantId, PolicyId, RequestedBy));
    }

    public void Approve(Guid approverId)
    {
        if (Status != ChangeRequestStatus.Submitted)
            throw new InvalidOperationException($"Cannot approve request in status: {Status}");

        Status = ChangeRequestStatus.Approved;
        ApproverId = approverId;

        // AddDomainEvent(new ChangeRequestApproved(Id, TenantId, approverId));
    }

    public void Reject(string reason)
    {
        if (Status != ChangeRequestStatus.Submitted)
            throw new InvalidOperationException($"Cannot reject request in status: {Status}");

        Status = ChangeRequestStatus.Rejected;
        RejectionReason = reason ?? throw new ArgumentNullException(nameof(reason));

        // AddDomainEvent(new ChangeRequestRejected(Id, TenantId, reason));
    }

    public void StartDeployment()
    {
        if (Status != ChangeRequestStatus.Approved)
            throw new InvalidOperationException($"Cannot start deployment for request in status: {Status}");

        Status = ChangeRequestStatus.DeploymentStarted;

        // AddDomainEvent(new DeploymentStarted(Id, TenantId, PolicyId));
    }

    public void CompleteDeployment(bool success, string? deploymentRef)
    {
        if (Status != ChangeRequestStatus.DeploymentStarted)
            throw new InvalidOperationException($"Cannot complete deployment for request in status: {Status}");

        Status = success ? ChangeRequestStatus.DeploymentCompleted : ChangeRequestStatus.DeploymentFailed;
        DeploymentRef = deploymentRef;

        // AddDomainEvent(new DeploymentCompleted(Id, TenantId, success, deploymentRef));
    }
}