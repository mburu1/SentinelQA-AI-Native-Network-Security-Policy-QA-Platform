using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Modules.ChangeManagement.Features.SubmitChangeRequest;

internal sealed class SubmitChangeRequestCommandHandler(
    IPolicyRepository policies,
    IChangeRequestRepository changeRequests,
    IUnitOfWork unitOfWork,
    ICommandPublisher commands,
    ICurrentUser currentUser,
    ICorrelationContext correlation) : IRequestHandler<SubmitChangeRequestCommand, Guid>
{
    public async Task<Guid> Handle(SubmitChangeRequestCommand request, CancellationToken cancellationToken)
    {
        var policy = await policies.GetByIdAsync(request.PolicyId, cancellationToken)
            ?? throw new NotFoundException($"Policy '{request.PolicyId}' was not found.");

        var changeRequest = ChangeRequest.Open(currentUser.TenantId, policy.Id, currentUser.UserId, request.Reason);
        changeRequest.Submit();

        await changeRequests.AddAsync(changeRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await commands.PublishAsync(new ValidateChangeRequestCommand(changeRequest.Id, policy.Id, currentUser.TenantId)
        {
            CorrelationId = correlation.CorrelationId
        }, cancellationToken);

        return changeRequest.Id;
    }
}