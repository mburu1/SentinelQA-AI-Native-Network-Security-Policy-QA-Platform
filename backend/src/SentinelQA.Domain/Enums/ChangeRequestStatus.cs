namespace SentinelQA.Domain.Enums;

public enum ChangeRequestStatus
{
    Draft,
    PendingValidation,
    PendingApproval,
    Approved,
    Rejected,
    Deploying,
    Deployed,
    Failed,
    RolledBack
}
