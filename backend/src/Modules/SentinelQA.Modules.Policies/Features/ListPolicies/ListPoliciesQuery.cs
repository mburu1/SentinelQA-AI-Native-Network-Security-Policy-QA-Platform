using MediatR;
using SentinelQA.Application.Common;

namespace SentinelQA.Modules.Policies.Features.ListPolicies;

public sealed record ListPoliciesQuery(Guid? FirewallId, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedResult<PolicySummary>>;

public sealed record PolicySummary(Guid Id, Guid FirewallId, string Name, int Version, string Status, DateTimeOffset UpdatedAt);