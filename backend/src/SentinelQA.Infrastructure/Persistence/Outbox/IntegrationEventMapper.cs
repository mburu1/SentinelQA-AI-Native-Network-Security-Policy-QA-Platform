using SentinelQA.Contracts.Events;
using SentinelQA.Contracts.Messaging;
using SentinelQA.Domain.Common;
using SentinelQA.Domain.Events;

namespace SentinelQA.Infrastructure.Persistence.Outbox;

/// <summary>
/// Maps internal domain events to external integration events.
/// Events without a mapping stay in-process only.
/// </summary>
public sealed class IntegrationEventMapper(
    ICorrelationContextAccessor correlation)
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent) =>
        domainEvent switch
        {
            PolicyCreated e =>
                new PolicyCreatedIntegrationEvent(
                    e.TenantId,
                    correlation.CorrelationId,
                    e.PolicyId,
                    e.FirewallId,
                    e.Name,
                    e.Version),

            ChangeRequestSubmitted e =>
                new ChangeRequestSubmittedIntegrationEvent(
                    e.TenantId,
                    correlation.CorrelationId,
                    e.ChangeRequestId,
                    e.PolicyId,
                    e.RequestedBy),

            ChangeRequestApproved e =>
                new ChangeRequestApprovedIntegrationEvent(
                    e.TenantId,
                    correlation.CorrelationId,
                    e.ChangeRequestId,
                    e.ApproverId),

            DeploymentCompleted e =>
                new DeploymentCompletedIntegrationEvent(
                    e.TenantId,
                    correlation.CorrelationId,
                    e.ChangeRequestId,
                    e.Success,
                    e.DeploymentRef),

            TestRunCompleted e =>
                new TestRunCompletedIntegrationEvent(
                    e.TenantId,
                    correlation.CorrelationId,
                    e.TestRunId,
                    e.Status.ToString(),
                    e.TotalCount,
                    e.PassedCount,
                    e.FailedCount),

            DefectCreated e =>
                new DefectCreatedIntegrationEvent(
                    e.TenantId,
                    correlation.CorrelationId,
                    e.DefectId,
                    e.Title,
                    e.Severity.ToString()),

            _ => null
        };
}

/// <summary>
/// Provides the ambient correlation identifier used to correlate
/// requests, domain events, integration events, and background work.
/// </summary>
public interface ICorrelationContextAccessor
{
    Guid CorrelationId { get; }
}

/// <summary>
/// Async-local correlation context shared by API middleware
/// and background workers.
/// </summary>
public sealed class AsyncLocalCorrelationContext
    : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<Guid> Current = new();

    public Guid CorrelationId =>
        Current.Value == Guid.Empty
            ? Guid.CreateVersion7()
            : Current.Value;

    public static IDisposable Begin(Guid correlationId)
    {
        var previous = Current.Value;
        Current.Value = correlationId;
        return new Scope(
            () => Current.Value = previous);
    }

    private sealed class Scope(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}