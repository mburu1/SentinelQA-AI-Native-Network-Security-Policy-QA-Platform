using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Domain.Entities;

public sealed class PolicyRule : Entity<Guid>
{
    public int Priority { get; private set; }
    public CidrBlock Source { get; private set; } = default!;
    public CidrBlock Destination { get; private set; } = default!;
    public Protocol Protocol { get; private set; }
    public PortRange Port { get; private set; } = PortRange.Any;
    public RuleAction Action { get; private set; }
    public Direction Direction { get; private set; }
    public bool LoggingEnabled { get; private set; }
    public string? Description { get; private set; }
    public string? Justification { get; private set; }

    private PolicyRule() { } // EF Core

    public PolicyRule(
        int priority, CidrBlock source, CidrBlock destination, Protocol protocol,
        PortRange port, RuleAction action, Direction direction,
        bool loggingEnabled, string? description = null, string? justification = null)
    {
        if (priority <= 0)
            throw new Exceptions.DomainException("Rule priority must be a positive integer.");

        Id = Guid.CreateVersion7();
        Priority = priority;
        Source = source;
        Destination = destination;
        Protocol = protocol;
        Port = port;
        Action = action;
        Direction = direction;
        LoggingEnabled = loggingEnabled;
        Description = description;
        Justification = justification;
    }

    /// <summary>True when this rule's scope fully covers the other rule's scope (used for shadow detection).</summary>
    public bool ScopeContains(PolicyRule other) =>
        Source.Contains(other.Source)
        && Destination.Contains(other.Destination)
        && ProtocolCovers(Protocol, other.Protocol)
        && Port.Contains(other.Port);

    /// <summary>True when both rules can match at least one common packet.</summary>
    public bool ScopesOverlap(PolicyRule other) =>
        Source.Overlaps(other.Source)
        && Destination.Overlaps(other.Destination)
        && ProtocolOverlaps(Protocol, other.Protocol)
        && Port.Overlaps(other.Port);

    private static bool ProtocolCovers(Protocol covering, Protocol covered) => covering == Protocol.Any || covering == covered;
    private static bool ProtocolOverlaps(Protocol a, Protocol b) => a == Protocol.Any || b == Protocol.Any || a == b;
}