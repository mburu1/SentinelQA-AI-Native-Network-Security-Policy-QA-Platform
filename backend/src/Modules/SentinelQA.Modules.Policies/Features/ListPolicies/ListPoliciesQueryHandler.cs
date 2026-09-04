using MediatR;
using Microsoft.EntityFrameworkCore;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;

namespace SentinelQA.Modules.Policies.Features.ListPolicies;

internal sealed class ListPoliciesQueryHandler(IPolicyRepository policies) : IRequestHandler<ListPoliciesQuery, PaginatedResult<PolicySummary>>
{
    public async Task<PaginatedResult<PolicySummary>> Handle(ListPoliciesQuery request, CancellationToken cancellationToken)
    {
        var query = policies.Query();

        if (request.FirewallId is not null)
            query = query.Where(p => p.FirewallId == request.FirewallId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PolicySummary(p.Id, p.FirewallId, p.Name, p.Version, p.Status.ToString(), p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PolicySummary>(items, totalCount, request.Page, request.PageSize);
    }
}