using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Entities;

public sealed class ChangeRequest
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid PolicyId { get; init; }
    public ChangeRequestStatus Status { get; set; }
}
