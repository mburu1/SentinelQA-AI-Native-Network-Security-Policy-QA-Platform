using System;
using System.Net;
using System.Net.Sockets;

namespace SentinelQA.Domain.ValueObjects;

public sealed record CidrBlock
{
    public string Value { get; init; } = default!;

    // Cache parsed state to avoid re-parsing on every Contains/Overlaps call
    private readonly IPAddress _networkAddress = default!;
    private readonly int _prefixLength;

    private CidrBlock() { }

    public CidrBlock(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CIDR block cannot be null or whitespace.", nameof(value));

        Parse(value.Trim(), out _networkAddress, out _prefixLength);

        // Force canonical representation (e.g., 192.168.1.1 becomes 192.168.1.1/32)
        Value = $"{_networkAddress}/{_prefixLength}";
    }

    public bool IsAny => Value is "0.0.0.0/0" or "::/0";

    public bool Contains(CidrBlock other)
    {
        if (IsAny) return true;
        if (other.IsAny) return false;
        if (_networkAddress.AddressFamily != other._networkAddress.AddressFamily) return false;
        if (_prefixLength > other._prefixLength) return false;

        return GetNetworkAddress(other._networkAddress, _prefixLength).Equals(_networkAddress);
    }

    public bool Overlaps(CidrBlock other)
    {
        if (IsAny || other.IsAny) return true;
        if (_networkAddress.AddressFamily != other._networkAddress.AddressFamily) return false;

        int smallerPrefix = Math.Min(_prefixLength, other._prefixLength);
        return GetNetworkAddress(_networkAddress, smallerPrefix)
              .Equals(GetNetworkAddress(other._networkAddress, smallerPrefix));
    }

    private static void Parse(string cidr, out IPAddress networkAddress, out int prefixLength)
    {
        ReadOnlySpan<char> span = cidr.AsSpan();
        int slashIndex = span.IndexOf('/');

        ReadOnlySpan<char> ipPart = slashIndex == -1 ? span : span[..slashIndex];

        if (!IPAddress.TryParse(ipPart, out var ip))
            throw new InvalidOperationException($"Invalid IP address format in CIDR: {cidr}");

        prefixLength = ip.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;

        if (slashIndex != -1)
        {
            ReadOnlySpan<char> prefixPart = span[(slashIndex + 1)..];
            if (!int.TryParse(prefixPart, out prefixLength) || prefixLength < 0 || prefixLength > (ip.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32))
            {
                throw new InvalidOperationException($"Invalid prefix length in CIDR: {cidr}");
            }
        }

        networkAddress = GetNetworkAddress(ip, prefixLength);
    }

    private static IPAddress GetNetworkAddress(IPAddress address, int prefixLength)
    {
        Span<byte> ipBytes = stackalloc byte[address.AddressFamily == AddressFamily.InterNetworkV6 ? 16 : 4];
        address.TryWriteBytes(ipBytes, out _);

        for (int i = 0; i < ipBytes.Length; i++)
        {
            int bitIndex = i * 8;
            if (bitIndex >= prefixLength)
            {
                ipBytes[i] = 0;
            }
            else if (prefixLength < bitIndex + 8)
            {
                int mask = (255 << (8 - (prefixLength - bitIndex))) & 255;
                ipBytes[i] = (byte)(ipBytes[i] & mask);
            }
        }

        return new IPAddress(ipBytes);
    }

    public override string ToString() => Value;
}