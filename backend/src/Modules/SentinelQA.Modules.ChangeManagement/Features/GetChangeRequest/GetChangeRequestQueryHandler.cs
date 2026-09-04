using MediatR;
using SentinelQA.Application.Abstractions.Persistence;

namespace SentinelQA.Modules.ChangeManagement.Features.GetChangeRequest;

internal sealed class GetChangeRequestQueryHandler(IChangeRequestRepository changeRequests)
    : IRequestHandler<GetChangeRequestQuery, ChangeRequestDetails?>
{
    public async Task<ChangeRequestDetails?> Handle(GetChangeRequestQuery request, CancellationToken cancellationToken)
    {
        var cr = await changeRequests.GetByIdAsync(request.ChangeRequestId, cancellationToken);
        if (cr is null)
            return null;

        return new ChangeRequestDetails(cr.Id, cr.PolicyId, cr.RequestedBy, cr.Reason, cr.State.ToString(),
            cr.ApproverId, cr.TestRunId, cr.DeploymentRef, cr.CreatedAt, cr.UpdatedAt);
    }
}