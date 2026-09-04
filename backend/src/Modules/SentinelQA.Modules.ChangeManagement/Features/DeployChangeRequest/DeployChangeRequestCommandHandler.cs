using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Modules.ChangeManagement.Features.DeployChangeRequest;

internal sealed class DeployChangeRequestCommandHandler(
    IChangeRequestRepository changeRequests,
    IPolicyRepository policies,
    IRepository<Firewall, Guid> firewalls,
    IUnitOfWork unitOfWork,
    ICommandPublisher commands,
    ICorrelationContext correlation) : IRequestHandler<DeployChangeRequestCommand, bool>
{
    public async Task<bool> Handle(DeployChangeRequestCommand request, CancellationToken cancellationToken)
    {
        var changeRequest = await changeRequests.GetByIdAsync(request.ChangeRequestId, cancellationToken)
            ?? throw new NotFoundException($"Change request '{request.ChangeRequestId}' was not found.");

        var policy = await policies.GetByIdAsync(changeRequest.PolicyId, cancellationToken)
            ?? throw new NotFoundException($"Policy '{changeRequest.PolicyId}' was not found.");

        // Domain invariant: only approved change requests can be deployed.
        changeRequest.BeginDeployment();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await commands.PublishAsync(new DeployFirewallPolicyCommand(changeRequest.Id, policy.Id, policy.FirewallId, changeRequest.TenantId)
        {
            CorrelationId = correlation.CorrelationId
        }, cancellationToken);

        return true;
    }
}