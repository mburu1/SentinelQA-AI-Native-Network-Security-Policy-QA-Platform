using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Domain.Aggregates;

public sealed class Network : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public CidrBlock Cidr { get; private set; } = default!;
    public NetworkType Type { get; private set; }
    public string? Description { get; private set; }

    private Network() { } // EF Core

    public static Network Define(Guid tenantId, string name, CidrBlock cidr, NetworkType type, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Network
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name.Trim(),
            Cidr = cidr,
            Type = type,
            Description = description
        };
    }
}