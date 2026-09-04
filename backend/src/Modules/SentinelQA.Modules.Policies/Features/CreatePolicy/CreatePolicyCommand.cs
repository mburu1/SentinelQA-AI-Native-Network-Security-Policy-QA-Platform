using MediatR;

namespace SentinelQA.Modules.Policies.Features.CreatePolicy;

public sealed record CreatePolicyCommand(
    Guid FirewallId,
    string Name,
    List<PolicyRuleDto> Rules) : IRequest<CreatePolicyResult>;

public sealed record PolicyRuleDto(
    int Priority,
    string Source,
    string Destination,
    string Protocol,
    string? Port,
    string Action,
    string Direction,
    bool LoggingEnabled,
    string? Description,
    string? Justification);

public sealed record CreatePolicyResult(Guid PolicyId, int Version, string Status);