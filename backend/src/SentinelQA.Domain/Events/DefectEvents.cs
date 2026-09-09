using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Events;

public sealed record DefectCreated(
    Guid DefectId,
    Guid TenantId,
    string Title,
    Severity Severity) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record DefectStatusChanged(
    Guid DefectId,
    Guid TenantId,
    DefectStatus From,
    DefectStatus To) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}