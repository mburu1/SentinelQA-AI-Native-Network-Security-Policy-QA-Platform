using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.ValueObjects;

public sealed record PortRange
{
    public int From { get; }
    public int To { get; }

    public PortRange(int from, int to)
    {
        if (from is < 0 or > 65535 || to is < 0 or > 65535)
            throw new DomainException($"Invalid port range {from}-{to}. Ports must be within 0-65535.");
        if (from > to)
            throw new DomainException($"Invalid port range {from}-{to}. 'From' cannot exceed 'To'.");

        From = from;
        To = to;
    }

    public static PortRange Any { get; } = new(0, 65535);
    public static PortRange Single(int port) => new(port, port);

    public static PortRange Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Contains('-'))
        {
            var parts = value.Split('-');
            return new PortRange(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        return Single(int.Parse(value));
    }

    public bool IsAny => From == 0 && To == 65535;
    public bool Contains(int port) => From <= port && port <= To;
    public bool Overlaps(PortRange other) => From <= other.To && other.From <= To;
    public bool Contains(PortRange other) => From <= other.From && To >= other.To;

    public override string ToString() => IsAny ? "any" : From == To ? From.ToString() : $"{From}-{To}";
}