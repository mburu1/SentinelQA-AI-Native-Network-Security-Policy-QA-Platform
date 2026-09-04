namespace SentinelQA.Api.Authorization;

public static class PolicyNames
{
    public const string CanDeployPolicy = nameof(CanDeployPolicy);
    public const string CanApproveChange = nameof(CanApproveChange);
    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanRunSecurityTests = nameof(CanRunSecurityTests);
    public const string CanViewAuditTrail = nameof(CanViewAuditTrail);
}

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string SecurityEngineer = nameof(SecurityEngineer);
    public const string QAEngineer = nameof(QAEngineer);
    public const string Developer = nameof(Developer);
    public const string Approver = nameof(Approver);
    public const string Viewer = nameof(Viewer);
}