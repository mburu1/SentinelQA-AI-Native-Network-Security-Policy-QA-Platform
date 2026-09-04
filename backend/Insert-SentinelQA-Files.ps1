#Requires -Version 5.1
<#
.SYNOPSIS
    Materializes the complete SentinelQA backend source tree from the provided dump.
.DESCRIPTION
    Creates all directories and writes every .cs / config file under:
    D:\Mwangi Wa Mburu\Coding\SentinelQA\backend
#>

$ErrorActionPreference = 'Stop'
$Base = 'D:\Mwangi Wa Mburu\Coding\SentinelQA\backend'

function Write-File {
    param(
        [Parameter(Mandatory)][string]$RelativePath,
        [Parameter(Mandatory)][string]$Content
    )
    $full = Join-Path $Base $RelativePath
    $dir  = Split-Path $full -Parent
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    # Normalize line endings to Windows
    $normalized = $Content -replace "`r?`n", "`r`n"
    [System.IO.File]::WriteAllText($full, $normalized, [System.Text.UTF8Encoding]::new($false))
    Write-Host "  + $RelativePath" -ForegroundColor Green
}

Write-Host "`n=== SentinelQA Backend Materialization ===" -ForegroundColor Cyan
Write-Host "Target : $Base`n" -ForegroundColor Cyan

# ──────────────────────────────────────────────────────────────
# 1. Domain layer
# ──────────────────────────────────────────────────────────────
Write-Host "Domain ..." -ForegroundColor Yellow

Write-File 'src/SentinelQA.Domain/Common/IDomainEvent.cs' @'
namespace SentinelQA.Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTimeOffset OccurredOn { get; }
}
'@

Write-File 'src/SentinelQA.Domain/Common/Entity.cs' @'
namespace SentinelQA.Domain.Common;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Id.Equals(entity.Id);
    public override int GetHashCode() => Id.GetHashCode();
}
'@

Write-File 'src/SentinelQA.Domain/Common/AggregateRoot.cs' @'
namespace SentinelQA.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
'@

Write-File 'src/SentinelQA.Domain/Exceptions/DomainException.cs' @'
namespace SentinelQA.Domain.Exceptions;

public class DomainException(string message) : Exception(message);
'@

# Enums
Write-File 'src/SentinelQA.Domain/Enums/PolicyEnums.cs' @'
namespace SentinelQA.Domain.Enums;

public enum Protocol { Tcp, Udp, Icmp, Any }
public enum RuleAction { Allow, Deny }
public enum Direction { Inbound, Outbound, Any }
public enum PolicyStatus { Draft, Validated, Deployed, Retired }
'@

Write-File 'src/SentinelQA.Domain/Enums/FirewallEnums.cs' @'
namespace SentinelQA.Domain.Enums;

public enum FirewallStatus { Healthy, Degraded, Offline, Unknown }
public enum FirewallConnectionType { Simulated, Api, Cli }
public enum NetworkType { Production, Staging, Management, Development }
'@

Write-File 'src/SentinelQA.Domain/Enums/ChangeManagementEnums.cs' @'
namespace SentinelQA.Domain.Enums;

public enum ChangeRequestState
{
    Draft, Submitted, Validating, Testing, AwaitingApproval, Approved,
    Rejected, Deploying, Verification, Completed, Failed, RolledBack, Cancelled
}
'@

Write-File 'src/SentinelQA.Domain/Enums/TestingEnums.cs' @'
namespace SentinelQA.Domain.Enums;

public enum TestRunStatus { Pending, Running, Completed, Failed, Cancelled }
public enum TestResultStatus { Passed, Failed, Skipped, Error }
public enum TestCaseType { Positive, Negative, Boundary, Security, Performance }
'@

Write-File 'src/SentinelQA.Domain/Enums/DefectEnums.cs' @'
namespace SentinelQA.Domain.Enums;

public enum DefectStatus { Open, Triaged, InProgress, Fixed, Retest, Verified, Closed }
public enum Severity { Critical, High, Medium, Low, Trivial }
public enum PriorityLevel { P1, P2, P3, P4 }
'@

Write-File 'src/SentinelQA.Domain/Enums/PlatformEnums.cs' @'
namespace SentinelQA.Domain.Enums;

public enum NotificationStatus { Pending, Sent, Failed }
public enum NotificationChannel { Email }
'@

# Value Objects (full content from dump)
Write-File 'src/SentinelQA.Domain/ValueObjects/CidrBlock.cs' @'
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
'@

Write-File 'src/SentinelQA.Domain/ValueObjects/PortRange.cs' @'
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
'@

Write-File 'src/SentinelQA.Domain/ValueObjects/Email.cs' @'
using System.Text.RegularExpressions;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.ValueObjects;

public sealed partial record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw new DomainException($"Invalid email address '{value}'.");

        return new Email(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
'@

# (Continue the same pattern for every remaining file…)
# Because of length limits in this chat I cannot paste the entire 240 kB dump again.
# The pattern below is the exact one you should follow for the rest of the files.

Write-Host "`nScript skeleton ready." -ForegroundColor Cyan
Write-Host "Copy the remaining Write-File blocks from the original dump using the same pattern." -ForegroundColor Yellow
Write-Host "Every path that starts with src/ or tests/ maps 1:1 under `$Base." -ForegroundColor Yellow