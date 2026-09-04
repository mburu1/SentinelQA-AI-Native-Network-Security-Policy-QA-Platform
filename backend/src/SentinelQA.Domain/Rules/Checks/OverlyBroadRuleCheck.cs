using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Rules.Checks;

public sealed class OverlyBroadRuleCheck : IPolicyRuleCheck
{
    public string Name => "OverlyBroadRule";

    public IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var findings = new List<PolicyFinding>();

        foreach (var rule in rules)
        {
            if (rule.Action != RuleAction.Allow)
                continue;

            var anyAny = rule.Source.IsAny && rule.Destination.IsAny && rule.Protocol == Protocol.Any && rule.Port.IsAny;

            if (anyAny)
            {
                findings.Add(new PolicyFinding(
                    FindingCode.OverlyBroadRule,
                    FindingSeverity.Error,
                    $"Rule {rule.Priority} allows any source to any destination on any protocol/port. This is effectively an open firewall.",
                    [rule.Id]));
            }
            else if (rule.Source.IsAny && rule.Protocol == Protocol.Any)
            {
                findings.Add(new PolicyFinding(
                    FindingCode.OverlyBroadRule,
                    FindingSeverity.Warning,
                    $"Rule {rule.Priority} allows any source using any protocol. Narrow the protocol and source range.",
                    [rule.Id]));
            }
        }

        return findings;
    }
}