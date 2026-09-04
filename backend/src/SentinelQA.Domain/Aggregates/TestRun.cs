using SentinelQA.Domain.Common;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.Aggregates;

public sealed class TestRun : AggregateRoot<Guid>
{
    private readonly List<TestResult> _results = [];

    public Guid TenantId { get; private set; }
    public Guid TestSuiteId { get; private set; }
    public string Environment { get; private set; } = default!;
    public TestRunStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public int TotalCount { get; private set; }
    public int PassedCount { get; private set; }
    public int FailedCount { get; private set; }
    public string? FailureReason { get; private set; }

    public IReadOnlyCollection<TestResult> Results => _results.AsReadOnly();

    private TestRun() { } // EF Core

    public static TestRun Queue(Guid tenantId, Guid testSuiteId, string environment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        return new TestRun
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            TestSuiteId = testSuiteId,
            Environment = environment.Trim().ToLowerInvariant(),
            Status = TestRunStatus.Pending
        };
    }

    public void Start()
    {
        if (Status != TestRunStatus.Pending)
            throw new DomainException($"Cannot start test run in status '{Status}'.");

        Status = TestRunStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TestRunStarted(Id, TenantId, TestSuiteId, Environment));
    }

    public void Record(TestResult result)
    {
        if (Status != TestRunStatus.Running)
            throw new DomainException("Results can only be recorded while the test run is running.");

        _results.Add(result);
        TotalCount++;
        if (result.Status == TestResultStatus.Passed) PassedCount++;
        if (result.Status is TestResultStatus.Failed or TestResultStatus.Error) FailedCount++;
    }

    public void Complete()
    {
        if (Status != TestRunStatus.Running)
            throw new DomainException($"Cannot complete test run in status '{Status}'.");

        Status = FailedCount > 0 ? TestRunStatus.Failed : TestRunStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TestRunCompleted(Id, TenantId, Status, TotalCount, PassedCount, FailedCount));
    }

    public void Cancel()
    {
        if (Status is TestRunStatus.Completed or TestRunStatus.Cancelled)
            throw new DomainException($"Cannot cancel test run in status '{Status}'.");

        Status = TestRunStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}