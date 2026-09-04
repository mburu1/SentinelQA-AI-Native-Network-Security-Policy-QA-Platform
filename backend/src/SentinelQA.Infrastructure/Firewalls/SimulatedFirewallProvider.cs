using Microsoft.Extensions.Logging;
using SentinelQA.Application.Abstractions.External;

namespace SentinelQA.Infrastructure.Firewalls;

/// <summary>
/// Deterministic simulated firewall so development and CI never depend on real hardware.
/// A policy named with 'fail' simulates a deployment failure for negative-path testing.
/// </summary>
public sealed class SimulatedFirewallProvider(ILogger<SimulatedFirewallProvider> logger) : IFirewallProvider
{
    public string ProviderName => "Simulated";

    public async Task<FirewallDeploymentResult> DeployPolicyAsync(FirewallDeploymentRequest request, CancellationToken cancellationToken = default)
    {
        await Task.Delay(Random.Shared.Next(150, 600), cancellationToken);

        if (request.PolicyName.Contains("fail", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Simulated deployment failure for policy {PolicyName}.", request.PolicyName);
            return new FirewallDeploymentResult(false, null, "Simulated deployment failure triggered by policy name.");
        }

        var deploymentRef = $"SIM-{request.PolicyId:N}"[..24];
        logger.LogInformation("Simulated deployment {DeploymentRef} succeeded for policy {PolicyName}.", deploymentRef, request.PolicyName);
        return new FirewallDeploymentResult(true, deploymentRef, null);
    }

    public async Task<FirewallRollbackResult> RollbackAsync(string deploymentRef, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return new FirewallRollbackResult(true, null);
    }

    public Task<FirewallHealthResult> GetStatusAsync(Guid firewallId, CancellationToken cancellationToken = default) =>
        Task.FromResult(new FirewallHealthResult("Healthy", "Simulated provider is always reachable."));
}