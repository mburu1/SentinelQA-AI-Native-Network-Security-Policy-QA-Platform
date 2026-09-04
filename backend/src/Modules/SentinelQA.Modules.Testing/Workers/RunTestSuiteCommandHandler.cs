using System.Diagnostics;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Infrastructure;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Documents;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Modules.Testing.Workers;

/// <summary>
/// RabbitMQ consumer: executes a test suite. The simulated executor fails any case
/// whose name contains 'fail', giving deterministic negative paths for demos and tests.
/// </summary>
internal sealed class RunTestSuiteCommandHandler(
    ITestRunRepository testRuns,
    ITestSuiteRepository testSuites,
    IDocumentStore documents,
    IUnitOfWork unitOfWork) : CommandHandlerBase<RunTestSuiteCommand>
{
    public override async Task HandleAsync(RunTestSuiteCommand command, CancellationToken cancellationToken)
    {
        var run = await testRuns.GetByIdAsync(command.TestRunId, cancellationToken)
            ?? throw new InvalidOperationException($"Test run '{command.TestRunId}' not found.");

        var suite = await testSuites.GetWithCasesAsync(command.TestSuiteId, cancellationToken)
            ?? throw new InvalidOperationException($"Test suite '{command.TestSuiteId}' not found.");

        run.Start();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var executions = new List<CaseExecution>();

        foreach (var testCase in suite.Cases)
        {
            var duration = Random.Shared.Next(20, 400);
            var failed = testCase.Name.Contains("fail", StringComparison.OrdinalIgnoreCase);

            run.Record(new TestResult(
                testCase.Id,
                testCase.Name,
                failed ? TestResultStatus.Failed : TestResultStatus.Passed,
                duration,
                failed ? $"Assertion failed for case '{testCase.Name}'. Expected: {testCase.ExpectedResult}." : null));

            executions.Add(new CaseExecution(testCase.Name, failed ? "failed" : "passed", duration, failed ? "assertion-failure" : null));
        }

        run.Complete();

        await documents.SaveAsync("testExecutionArtifacts", new TestExecutionArtifact
        {
            TestRunId = run.Id,
            TenantId = command.TenantId,
            Environment = command.Environment,
            TraceId = Activity.Current?.TraceId.ToString(),
            Cases = executions
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}