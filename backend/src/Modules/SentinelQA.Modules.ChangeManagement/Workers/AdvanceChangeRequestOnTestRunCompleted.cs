using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;

namespace SentinelQA.Modules.ChangeManagement.Workers;

/// <summary>When a linked test run finishes, the change request advances or fails.</summary>
internal sealed class AdvanceChangeRequestOnTestRunCompleted(
    IChangeRequestRepository changeRequests,
    IUnitOfWork unitOfWork) : INotificationHandler<TestRunCompleted>
{
    public async Task Handle(TestRunCompleted notification, CancellationToken cancellationToken)
    {
        var changeRequest = await changeRequests.GetByTestRunIdAsync(notification.TestRunId, cancellationToken);
        if (changeRequest is null || changeRequest.State != ChangeRequestState.Testing)
            return;

        changeRequest.CompleteTesting(notification.Status == TestRunStatus.Completed);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}