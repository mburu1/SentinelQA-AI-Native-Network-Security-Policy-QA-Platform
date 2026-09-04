using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Events;

public sealed record DefectCreated(Guid DefectId, Guid TenantId, string Title, Severity Severity) : DomainEventBase;
public sealed record DefectStatusChanged(Guid DefectId, Guid TenantId, DefectStatus From, DefectStatus To) : DomainEventBase;