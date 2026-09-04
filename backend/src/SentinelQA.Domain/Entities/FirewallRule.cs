namespace SentinelQA.Domain.Entities;

public sealed class FirewallRule
{
    public Guid Id { get; init; }
    public Guid FirewallId { get; init; }
    public required string SourceCidr { get; set; }
    public required string DestinationCidr { get; set; }
    public required string Protocol { get; set; }
    public int Port { get; set; }
    public required string Action { get; set; }
    public int Priority { get; set; }
}
