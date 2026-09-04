using MediatR;
using SentinelQA.Application.Abstractions.Persistence;

namespace SentinelQA.Modules.Policies.Features.GetPolicy;

internal sealed class GetPolicyQueryHandler(IPolicyRepository policies) : IRequestHandler<GetPolicyQuery, PolicyDetails?>
{
    public async Task<PolicyDetails?> Handle(GetPolicyQuery request, CancellationToken cancellationToken)
    {
        var policy = await policies.GetWithRulesAsync(request.PolicyId, cancellationToken);
        if (policy is null)
            return null;

        return new PolicyDetails(
            policy.Id, policy.FirewallId, policy.Name, policy.Version, policy.Status.ToString(),
            policy.CreatedAt, policy.UpdatedAt,
            policy.Rules.Select(r => new PolicyRuleDetails(
                r.Id, r.Priority, r.Source.ToString(), r.Destination.ToString(), r.Protocol.ToString(),
                r.Port.ToString(), r.Action.ToString(), r.Direction.ToString(), r.LoggingEnabled, r.Description)).ToList());
    }
}