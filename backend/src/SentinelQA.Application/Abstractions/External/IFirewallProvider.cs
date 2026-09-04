namespace SentinelQA.Application.Abstractions.External;

public interface IFirewallProvider
{
    string ProviderName { get; }
    Task<FirewallDeploymentResult> DeployPolicyAsync(FirewallDeploymentRequest request, CancellationToken cancellationToken = default);
    Task<FirewallRollbackResult> RollbackAsync(string deploymentRef, CancellationToken cancellationToken = default);
    Task<FirewallHealthResult> GetStatusAsync(Guid firewallId, CancellationToken cancellationToken = default);
}

public sealed record FirewallDeploymentRequest(Guid FirewallId, Guid PolicyId, string PolicyName, int PolicyVersion);
public sealed record FirewallDeploymentResult(bool Success, string? DeploymentRef, string? Error);
public sealed record FirewallRollbackResult(bool Success, string? Error);
public sealed record FirewallHealthResult(string Status, string? Message);