using FluentAssertions;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Rules;
using SentinelQA.Domain.ValueObjects;
using Xunit;

namespace SentinelQA.UnitTests.Domain.Rules;

public sealed class PolicyAnalyzerTests
{
    private readonly PolicyAnalyzer _analyzer = new();

    private static PolicyRule Rule(int priority, string source, string destination, RuleAction action, int? port = 443, Protocol protocol = Protocol.Tcp) =>
        new(priority, CidrBlock.Parse(source), CidrBlock.Parse(destination), protocol,
            port is null ? PortRange.Any : PortRange.Single(port.Value), action, Direction.Inbound, loggingEnabled: true);

    [Fact]
    public void Detects_public_ssh_exposure_as_critical()
    {
        var rules = new[] { Rule(10, "0.0.0.0/0", "10.0.10.15/32", RuleAction.Allow, port: 22) };

        var result = _analyzer.Analyze(rules);

        result.HasCritical.Should().BeTrue();
        result.Findings.Should().Contain(f => f.Code == FindingCode.PublicExposure);
    }

    [Fact]
    public void Detects_shadowed_rule()
    {
        var rules = new[]
        {
            Rule(10, "10.0.0.0/16", "192.168.1.0/24", RuleAction.Allow),
            Rule(11, "10.0.10.0/24", "192.168.1.0/24", RuleAction.Deny)
        };

        var result = _analyzer.Analyze(rules);

        result.Findings.Should().Contain(f => f.Code == FindingCode.UnreachableRule);
    }

    [Fact]
    public void Detects_duplicate_rules()
    {
        var rules = new[]
        {
            Rule(10, "10.0.0.0/24", "10.1.0.0/24", RuleAction.Allow),
            Rule(20, "10.0.0.0/24", "10.1.0.0/24", RuleAction.Allow)
        };

        var result = _analyzer.Analyze(rules);

        result.Findings.Should().Contain(f => f.Code == FindingCode.DuplicateRule);
    }

    [Fact]
    public void Allows_internal_ssh_when_scoped()
    {
        var rules = new[] { Rule(10, "10.10.0.0/16", "10.20.0.10/32", RuleAction.Allow, port: 22) };

        var result = _analyzer.Analyze(rules);

        result.HasCritical.Should().BeFalse();
        result.Findings.Should().NotContain(f => f.Code == FindingCode.PublicExposure);
    }

    [Fact]
    public void Flags_any_any_allow_as_error()
    {
        var rules = new[] { Rule(10, "0.0.0.0/0", "0.0.0.0/0", RuleAction.Allow, port: null, protocol: Protocol.Any) };

        var result = _analyzer.Analyze(rules);

        result.IsValid.Should().BeFalse();
        result.Findings.Should().Contain(f => f.Code == FindingCode.OverlyBroadRule && f.Severity == FindingSeverity.Error);
    }
}