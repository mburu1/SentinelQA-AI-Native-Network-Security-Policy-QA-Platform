using MediatR;

namespace SentinelQA.Modules.Testing.Features.GetTestRun;

public sealed record GetTestRunQuery(Guid TestRunId) : IRequest<TestRunDetails?>;

public sealed record TestRunDetails(
    Guid Id, Guid TestSuiteId, string Environment, string Status,
    int Total, int Passed, int Failed, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt,
    IReadOnlyList<TestResultDetails> Results);

public sealed record TestResultDetails(Guid Id, string CaseName, string Status, long DurationMs, string? FailureMessage);