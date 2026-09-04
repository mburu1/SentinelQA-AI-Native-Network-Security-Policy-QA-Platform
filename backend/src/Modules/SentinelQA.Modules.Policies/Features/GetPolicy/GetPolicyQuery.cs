using MediatR;

namespace SentinelQA.Modules.Policies.Features.GetPolicy;

public sealed record GetPolicyQuery(Guid PolicyId) : IRequest<PolicyDetails?>;

public sealed record PolicyDetails(
    Guid Id, Guid FirewallId, string Name, int Version, string Status,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, IReadOnlyList<PolicyRuleDetails> Rules);

public sealed record PolicyRuleDetails(
    Guid Id, int Priority, string Source, string Destination, string Protocol,
    string Port, string Action, string Direction, bool LoggingEnabled, string? Description);