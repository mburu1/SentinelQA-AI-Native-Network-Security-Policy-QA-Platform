using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Modules.Policies.Features.CreatePolicy;

internal sealed class CreatePolicyCommandHandler(
    IRepository<Firewall, Guid> firewalls,
    IPolicyRepository policies,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<CreatePolicyCommand, CreatePolicyResult>
{
    public async Task<CreatePolicyResult> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        var firewall = await firewalls.GetByIdAsync(request.FirewallId, cancellationToken)
            ?? throw new NotFoundException($"Firewall '{request.FirewallId}' was not found.");

        var rules = request.Rules.Select(dto => new PolicyRule(
            dto.Priority,
            CidrBlock.Parse(dto.Source),
            CidrBlock.Parse(dto.Destination),
            Enum.Parse<Protocol>(dto.Protocol, ignoreCase: true),
            string.IsNullOrWhiteSpace(dto.Port) ? PortRange.Any : PortRange.Parse(dto.Port),
            Enum.Parse<RuleAction>(dto.Action, ignoreCase: true),
            Enum.Parse<Direction>(dto.Direction, ignoreCase: true),
            dto.LoggingEnabled,
            dto.Description,
            dto.Justification));

        var policy = Policy.Create(currentUser.TenantId, firewall.Id, request.Name, rules);

        await policies.AddAsync(policy, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreatePolicyResult(policy.Id, policy.Version, policy.Status.ToString());
    }
}