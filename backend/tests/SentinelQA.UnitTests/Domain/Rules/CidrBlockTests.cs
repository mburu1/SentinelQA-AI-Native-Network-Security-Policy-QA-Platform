/*using System.Net;
using System.Net.Sockets;

namespace SentinelQA.Domain.ValueObjects;

public sealed record CidrBlock
{
    public IPAddress NetworkAddress { get; }
    public int PrefixLength { get; }

    private CidrBlock(IPAddress networkAddress, int prefixLength)
    {
        NetworkAddress = networkAddress;
        PrefixLength = prefixLength;
    }

    public static CidrBlock Parse(string cidr)
        => TryParse(cidr, out var result)
            ? result
            : throw new ArgumentException($"Invalid CIDR block: '{cidr}'.", nameof(cidr));

    public static bool TryParse(string? cidr, out CidrBlock result)
    {
        result = null!;
        if (string.IsNullOrWhiteSpace(cidr)) return false;

        var parts = cidr.Split('/');
        if (parts.Length != 2) return false;
        if (!IPAddress.TryParse(parts[0], out var ip)) return false;          // rejects 10.0.0.999
        if (ip.AddressFamily != AddressFamily.InterNetwork) return false;     // IPv4 for now
        if (!int.TryParse(parts[1], out var prefix)) return false;
        if (prefix is < 0 or > 32) return false;                              // rejects /33

        var mask = prefix == 0 ? 0u : 0xFFFFFFFFu << (32 - prefix);
        var network = ToUInt32(ip) & mask;                                    // canonicalize host bits
        result = new CidrBlock(FromUInt32(network), prefix);
        return true;
    }

    public bool IsSingleHost => PrefixLength == 32;
    public bool IsAny => PrefixLength == 0 && NetworkAddress.Equals(IPAddress.Any);

    public bool Contains(IPAddress address)
    {
        var mask = PrefixLength == 0 ? 0u : 0xFFFFFFFFu << (32 - PrefixLength);
        return (ToUInt32(address) & mask) == ToUInt32(NetworkAddress);
    }

    public bool Contains(CidrBlock other)
        => other.PrefixLength >= PrefixLength && Contains(other.NetworkAddress);

    public bool Overlaps(CidrBlock other) => Contains(other) || other.Contains(this);

    public override string ToString() => $"{NetworkAddress}/{PrefixLength}";

    private static uint ToUInt32(IPAddress a)
    {
        var b = a.GetAddressBytes();
        return (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]);
    }

    private static IPAddress FromUInt32(uint v)
        => new(new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v });
}*/