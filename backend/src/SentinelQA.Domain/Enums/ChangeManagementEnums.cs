namespace SentinelQA.Domain.Enums;

public enum ChangeRequestState
{
    Draft, Submitted, Validating, Testing, AwaitingApproval, Approved,
    Rejected, Deploying, Verification, Completed, Failed, RolledBack, Cancelled
}