using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;

namespace SentinelQA.Modules.Testing.Workers;

/// <summary>Automated defect creation for failed runs (spec §35, step 6).</summary>
internal sealed class CreateDefectOnFailedTestRun(
    IRepository<Defect, Guid> defects,
    IUnitOfWork unitOfWork) : INotificationHandler<TestRunCompleted>
{
    public async Task Handle(TestRunCompleted notification, CancellationToken cancellationToken)
    {
        if (notification.Failed == 0)
            return;

        var defect = Defect.Report(
            tenantId: notification.TenantId,
            title: $"Automated failure: {notification.Failed}/{notification.Total} cases failed in test run {notification.TestRunId}",
            description: "Automatically created from a failed automated test run.",
            severity: Severity.High,
            priority: PriorityLevel.P2,
            environment: "qa",
            reportedBy: null,
            testRunId: notification.TestRunId,
            component: "AutomatedTesting",
            steps: $"Re-run test run {notification.TestRunId}.",
            expected: "All test cases pass.",
            actual: $"{notification.Failed} test case(s) failed.");

        await defects.AddAsync(defect, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}