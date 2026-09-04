using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Contracts.Messages;

public abstract record CommandBase : ICommand
{
    public Guid CommandId { get; init; } = Guid.CreateVersion7();
    public Guid CorrelationId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record RunTestSuiteCommand(Guid TestRunId, Guid TestSuiteId, string Environment, Guid TenantId) : CommandBase;

public sealed record AnalyzeFailureWithAiCommand(Guid TestRunId, Guid TenantId, string FailureSummary) : CommandBase;