using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Rules;

namespace SentinelQA.Modules.ChangeManagement.Workers;

/// <summary>
/// RabbitMQ consumer: validates the policy, then either rejects the change
/// or kicks off automated testing (creating an ad-hoc suite when none exists).
/// </summary>
internal sealed class ValidateChangeRequestCommandHandler(
    IChangeRequestRepository changeRequests,
    IPolicyRepository policies,
    ITestSuiteRepository testSuites,
    IUnitOfWork unitOfWork,
    ICommandPublisher commands,
    PolicyAnalyzer analyzer,
    ICorrelationContext correlation) : CommandHandlerBase<ValidateChangeRequestCommand>(/* see note */)
{
    public override async Task HandleAsync(ValidateChangeRequestCommand command, CancellationToken cancellationToken)
    {
        var changeRequest = await changeRequests.GetByIdAsync(command.ChangeRequestId, cancellationToken)
            ?? throw new InvalidOperationException($"Change request '{command.ChangeRequestId}' not found.");

        var policy = await policies.GetWithRulesAsync(command.PolicyId, cancellationToken)
            ?? throw new InvalidOperationException($"Policy '{command.PolicyId}' not found.");

        changeRequest.BeginValidation();

        var analysis = analyzer.Analyze([.. policy.Rules]);
        changeRequest.CompleteValidation(analysis.IsValid);

        if (!analysis.IsValid)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        // Ensure a regression suite exists; otherwise create a smoke suite for this change.
        var suite = await FindOrCreateSuiteAsync(changeRequest, cancellationToken);

        var testRun = TestRun.Queue(command.TenantId, suite.Id, "qa");
        changeRequest.AttachTestRun(testRun.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await commands.PublishAsync(new RunTestSuiteCommand(testRun.Id, suite.Id, "qa", command.TenantId)
        {
            CorrelationId = correlation.CorrelationId
        }, cancellationToken);
    }

    private async Task<TestSuite> FindOrCreateSuiteAsync(ChangeRequest changeRequest, CancellationToken cancellationToken)
    {
        var existing = testSuites.Query().FirstOrDefault(s => s.IsRegression && s.TenantId == changeRequest.TenantId);
        if (existing is not null)
            return existing;

        var suite = TestSuite.Define(changeRequest.TenantId, "Change smoke suite", isRegression: true,
        [
            new TestCase("Policy analysis passes", TestCaseType.Positive, "Run policy analyzer", "No critical findings"),
            new TestCase("No public exposure", TestCaseType.Security, "Check allow rules with 0.0.0.0/0 source", "No critical findings"),
            new TestCase("Deployment simulation succeeds", TestCaseType.Positive, "Deploy via simulated provider", "200/202")
        ]);

        await testSuites.AddAsync(suite, cancellationToken);
        return suite;
    }
}