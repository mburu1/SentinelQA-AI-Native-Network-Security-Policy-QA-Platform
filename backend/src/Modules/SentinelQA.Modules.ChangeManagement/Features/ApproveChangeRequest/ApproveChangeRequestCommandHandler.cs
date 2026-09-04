using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;

namespace SentinelQA.Modules.ChangeManagement.Features.ApproveChangeRequest;

internal sealed class ApproveChangeRequestCommandHandler(
    IChangeRequestRepository changeRequests,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<ApproveChangeRequestCommand, bool>
{
    public async Task<bool> Handle(ApproveChangeRequestCommand request, CancellationToken cancellationToken)
    {
        var changeRequest = await changeRequests.GetByIdAsync(request.ChangeRequestId, cancellationToken)
            ?? throw new NotFoundException($"Change request '{request.ChangeRequestId}' was not found.");

        changeRequest.Approve(currentUser.UserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}