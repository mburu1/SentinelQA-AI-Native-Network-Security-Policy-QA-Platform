using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Entities;

public sealed class TestResult : Entity<Guid>
{
    public Guid? TestCaseId { get; private set; }
    public string CaseName { get; private set; } = default!;
    public TestResultStatus Status { get; private set; }
    public long DurationMs { get; private set; }
    public string? FailureMessage { get; private set; }

    private TestResult() { } // EF Core

    public TestResult(Guid? testCaseId, string caseName, TestResultStatus status, long durationMs, string? failureMessage = null)
    {
        Id = Guid.CreateVersion7();
        TestCaseId = testCaseId;
        CaseName = caseName;
        Status = status;
        DurationMs = durationMs;
        FailureMessage = failureMessage;
    }
}