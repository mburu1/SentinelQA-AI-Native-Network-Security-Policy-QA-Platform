using MediatR;
using SentinelQA.Application.Abstractions.Persistence;

namespace SentinelQA.Modules.Testing.Features.GetTestRun;

internal sealed class GetTestRunQueryHandler(ITestRunRepository testRuns) : IRequestHandler<GetTestRunQuery, TestRunDetails?>
{
    public async Task<TestRunDetails?> Handle(GetTestRunQuery request, CancellationToken cancellationToken)
    {
        var run = await testRuns.GetWithResultsAsync(request.TestRunId, cancellationToken);
        if (run is null)
            return null;

        return new TestRunDetails(
            run.Id, run.TestSuiteId, run.Environment, run.Status.ToString(),
            run.TotalCount, run.PassedCount, run.FailedCount, run.StartedAt, run.CompletedAt,
            run.Results.Select(r => new TestResultDetails(r.Id, r.CaseName, r.Status.ToString(), r.DurationMs, r.FailureMessage)).ToList());
    }
}