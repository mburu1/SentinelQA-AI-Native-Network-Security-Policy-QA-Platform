namespace SentinelQA.Domain.Entities;

public sealed class Firewall
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public required string Name { get; set; }
    public required string Hostname { get; set; }
}
