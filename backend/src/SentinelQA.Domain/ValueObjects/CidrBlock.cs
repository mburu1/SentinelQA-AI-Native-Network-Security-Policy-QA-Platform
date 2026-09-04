using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.ValueObjects;

public sealed record CidrBlock
{
    public string Address { get; }
    public int PrefixLength { get; }
    public AddressFamily Family { get; }

    private readonly UInt128 _first;
    private readonly UInt128 _last;

    private CidrBlock(IPAddress address, int prefixLength)
    {
        Family = address.AddressFamily;
        var bits = BitsFor(Family);

        if (prefixLength < 0 || prefixLength > bits)
            throw new DomainException($"Invalid CIDR prefix length '{prefixLength}'. Max for {Family} is {bits}.");

        PrefixLength = prefixLength;
        var mask = MaskFor(prefixLength, bits);
        _first = ToUInt128(address) & mask;
        _last = _first | (~mask & AddressMask(bits));

        // Normalize to the network address so equality is range-based.
        Address = new IPAddress(_first.ToBigEndianBytes(bits)).ToString();
    }

    public static CidrBlock Parse(string cidr)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cidr);

        var parts = cidr.Split('/');
        if (parts.Length != 2)
            throw new DomainException($"Invalid CIDR format '{cidr}'. Expected 'address/prefix'.");

        if (!IPAddress.TryParse(parts[0], out var ip))
            throw new DomainException($"Invalid IP address '{parts[0]}' in CIDR '{cidr}'.");

        if (!int.TryParse(parts[1], out var prefix))
            throw new DomainException($"Invalid prefix length '{parts[1]}' in CIDR '{cidr}'.");

        return new CidrBlock(ip, prefix);
    }

    public static bool TryParse(string cidr, out CidrBlock? block)
    {
        try { block = Parse(cidr); return true; }
        catch { block = null; return false; }
    }

    public bool IsAny => PrefixLength == 0;
    public bool IsSingleHost => PrefixLength == BitsFor(Family);
    public UInt128 FirstAddress => _first;
    public UInt128 LastAddress => _last;

    public bool Overlaps(CidrBlock other) =>
        Family == other.Family && _first <= other._last && other._first <= _last;

    public bool Contains(CidrBlock other) =>
        Family == other.Family && _first <= other._first && _last >= other._last;

    public override string ToString() => $"{Address}/{PrefixLength}";

    private static int BitsFor(AddressFamily family) => family == AddressFamily.InterNetwork ? 32 : 128;
    private static UInt128 MaskFor(int prefix, int bits) => prefix == 0 ? UInt128.Zero : UInt128.MaxValue << (bits - prefix);
    private static UInt128 AddressMask(int bits) => bits == 128 ? UInt128.MaxValue : (UInt128.One << bits) - UInt128.One;

    private static UInt128 ToUInt128(IPAddress address)
    {
        Span<byte> raw = stackalloc byte[16];
        address.TryWriteBytes(raw, out var written);

        var value = UInt128.Zero;
        for (var i = 0; i < written; i++)
            value = (value << 8) | raw[i];

        return value;
    }
}

internal static class UInt128Extensions
{
    public static byte[] ToBigEndianBytes(this UInt128 value, int bits)
    {
        Span<byte> buffer = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(buffer, value);
        return buffer[(16 - bits / 8)..].ToArray();
    }
}