using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Events;

public sealed record TestRunStarted(Guid TestRunId, Guid TenantId, Guid TestSuiteId, string Environment) : DomainEventBase;
public sealed record TestRunCompleted(Guid TestRunId, Guid TenantId, TestRunStatus Status, int Total, int Passed, int Failed) : DomainEventBase;