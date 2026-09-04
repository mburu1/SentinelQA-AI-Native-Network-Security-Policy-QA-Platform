namespace SentinelQA.Contracts.Messages;

public sealed record ValidateChangeRequestCommand(Guid ChangeRequestId, Guid PolicyId, Guid TenantId) : CommandBase;

public sealed record DeployFirewallPolicyCommand(Guid ChangeRequestId, Guid PolicyId, Guid FirewallId, Guid TenantId) : CommandBase;

public sealed record RollbackDeploymentCommand(Guid ChangeRequestId, Guid TenantId, string? DeploymentRef) : CommandBase;