using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Aggregates;

public sealed class AuditEntry : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string EntityType { get; private set; } = default!;
    public string? EntityId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string? DetailsJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEntry() { } // EF Core

    public static AuditEntry Record(Guid tenantId, Guid? userId, string action, string entityType, string? entityId, Guid correlationId, string? detailsJson = null) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            CorrelationId = correlationId,
            DetailsJson = detailsJson,
            OccurredAt = DateTimeOffset.UtcNow
        };
}