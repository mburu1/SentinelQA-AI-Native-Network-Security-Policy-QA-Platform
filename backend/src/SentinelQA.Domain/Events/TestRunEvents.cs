using System;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Events;

public sealed record TestRunStarted(
    Guid TestRunId,
    Guid TenantId,
    Guid TestSuiteId,
    string Environment) : DomainEvent;

public sealed record TestRunCompleted(
    Guid TestRunId,
    Guid TenantId,
    TestRunStatus Status,
    int TotalCount,
    int PassedCount,
    int FailedCount) : DomainEvent;