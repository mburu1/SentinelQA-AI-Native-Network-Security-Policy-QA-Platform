using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.Aggregates;

public sealed class Firewall : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Vendor { get; private set; } = default!;
    public string Environment { get; private set; } = default!;
    public FirewallStatus Status { get; private set; }
    public FirewallConnectionType ConnectionType { get; private set; }
    public DateTimeOffset? LastHealthCheckAt { get; private set; }

    private Firewall() { } // EF Core

    public static Firewall Register(Guid tenantId, string name, string vendor, string environment, FirewallConnectionType connectionType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(vendor);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);

        return new Firewall
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name.Trim(),
            Vendor = vendor.Trim(),
            Environment = environment.Trim().ToLowerInvariant(),
            Status = FirewallStatus.Unknown,
            ConnectionType = connectionType
        };
    }

    public void RecordHealth(FirewallStatus status)
    {
        if (status == FirewallStatus.Unknown)
            throw new DomainException("Health checks must report a definitive status.");

        Status = status;
        LastHealthCheckAt = DateTimeOffset.UtcNow;
    }
}