using System;
using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.Aggregates;

public sealed class ChangeRequest : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid PolicyId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid? ApproverId { get; private set; }
    public Guid? TestRunId { get; private set; }
    public ChangeRequestStatus Status { get; private set; }
    public string Reason { get; private set; } = null!;
    public string? RejectionReason { get; private set; }
    public string? DeploymentRef { get; private set; }

    private ChangeRequest() { } // EF Core

    public static ChangeRequest Open(Guid tenantId, Guid policyId, Guid requestedBy, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason is required.");

        var cr = new ChangeRequest
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            PolicyId = policyId,
            RequestedBy = requestedBy,
            Reason = reason.Trim(),
            Status = ChangeRequestStatus.Draft
        };

        // cr.AddDomainEvent(new ChangeRequestOpened(cr.Id, tenantId, policyId, requestedBy));
        return cr;
    }

    public void Submit()
    {
        EnsureStatus(ChangeRequestStatus.Draft, nameof(Submit));
        Status = ChangeRequestStatus.Submitted;
        // AddDomainEvent(new ChangeRequestSubmitted(Id, TenantId, PolicyId, RequestedBy));
    }

    public void BeginValidation()
    {
        EnsureStatus(ChangeRequestStatus.Submitted, nameof(BeginValidation));
        Status = ChangeRequestStatus.Validating;
    }

    public void CompleteValidation(bool passed)
    {
        EnsureStatus(ChangeRequestStatus.Validating, nameof(CompleteValidation));
        Status = passed ? ChangeRequestStatus.Validated : ChangeRequestStatus.Failed;
    }

    public void AttachTestRun(Guid testRunId)
    {
        if (testRunId == Guid.Empty)
            throw new DomainException("TestRunId cannot be empty.");

        if (Status is not (ChangeRequestStatus.Validated or ChangeRequestStatus.Testing))
            throw new DomainException($"Cannot attach test run in status: {Status}");

        TestRunId = testRunId;
        Status = ChangeRequestStatus.Testing;
    }

    public void CompleteTesting(bool passed)
    {
        EnsureStatus(ChangeRequestStatus.Testing, nameof(CompleteTesting));
        Status = passed ? ChangeRequestStatus.Tested : ChangeRequestStatus.Failed;
    }

    public void Approve(Guid approverId)
    {
        if (approverId == Guid.Empty)
            throw new DomainException("ApproverId cannot be empty.");

        if (approverId == RequestedBy)
            throw new DomainException("Requester cannot approve own change (segregation of duties).");

        if (Status is not (ChangeRequestStatus.Tested or ChangeRequestStatus.Validated))
            throw new DomainException($"Cannot approve request in status: {Status}");

        ApproverId = approverId;
        Status = ChangeRequestStatus.Approved;
        // AddDomainEvent(new ChangeRequestApproved(Id, TenantId, approverId));
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required.");

        if (Status is not (ChangeRequestStatus.Submitted or ChangeRequestStatus.Validated or ChangeRequestStatus.Tested))
            throw new DomainException($"Cannot reject request in status: {Status}");

        RejectionReason = reason.Trim();
        Status = ChangeRequestStatus.Rejected;
        // AddDomainEvent(new ChangeRequestRejected(Id, TenantId, reason));
    }

    public void BeginDeployment()
    {
        EnsureStatus(ChangeRequestStatus.Approved, nameof(BeginDeployment));
        Status = ChangeRequestStatus.Deploying;
        // AddDomainEvent(new DeploymentStarted(Id, TenantId, PolicyId));
    }

    public void CompleteDeployment(bool success, string? deploymentRef)
    {
        EnsureStatus(ChangeRequestStatus.Deploying, nameof(CompleteDeployment));

        if (success && string.IsNullOrWhiteSpace(deploymentRef))
            throw new DomainException("DeploymentRef is required on successful deployment.");

        DeploymentRef = deploymentRef?.Trim();
        Status = success ? ChangeRequestStatus.Deployed : ChangeRequestStatus.Failed;
        // AddDomainEvent(new DeploymentCompleted(Id, TenantId, success, deploymentRef));
    }

    public void CompleteVerification(bool passed)
    {
        EnsureStatus(ChangeRequestStatus.Deployed, nameof(CompleteVerification));
        Status = passed ? ChangeRequestStatus.Completed : ChangeRequestStatus.Failed;
    }

    public void Retry()
    {
        if (Status != ChangeRequestStatus.Failed)
            throw new DomainException($"Cannot retry request in status: {Status}");

        // Restart from testing stage (matches test expectations)
        Status = ChangeRequestStatus.Testing;
        TestRunId = null;
    }

    private void EnsureStatus(ChangeRequestStatus expected, string operation)
    {
        if (Status != expected)
            throw new DomainException($"Cannot {operation} request in status: {Status}");
    }
}