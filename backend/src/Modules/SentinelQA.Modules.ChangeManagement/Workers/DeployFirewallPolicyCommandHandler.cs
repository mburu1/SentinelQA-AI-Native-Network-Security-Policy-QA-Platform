using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.External;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Modules.ChangeManagement.Workers;

/// <summary>RabbitMQ consumer: deploys through the firewall provider abstraction, then runs simulated verification.</summary>
internal sealed class DeployFirewallPolicyCommandHandler(
    IChangeRequestRepository changeRequests,
    IPolicyRepository policies,
    IFirewallProvider firewall,
    IUnitOfWork unitOfWork) : CommandHandlerBase<DeployFirewallPolicyCommand>
{
    public override async Task HandleAsync(DeployFirewallPolicyCommand command, CancellationToken cancellationToken)
    {
        var changeRequest = await changeRequests.GetByIdAsync(command.ChangeRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Change request '{command.ChangeRequestId}' not found.");

        var policy = await policies.GetByIdAsync(command.PolicyId, cancellationToken)
            ?? throw new InvalidOperationException($"Policy '{command.PolicyId}' not found.");

        var result = await firewall.DeployPolicyAsync(
            new FirewallDeploymentRequest(command.FirewallId, policy.Id, policy.Name, policy.Version),
            cancellationToken);

        changeRequest.CompleteDeployment(result.Success, result.DeploymentRef);

        if (result.Success)
        {
            policy.MarkDeployed();

            // Simulated post-deployment verification. A real deployment would run
            // a verification test suite here before completing.
            changeRequest.CompleteVerification(passed: true);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}