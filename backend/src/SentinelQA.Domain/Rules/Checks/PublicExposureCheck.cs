using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Rules.Checks;

/// <summary>
/// Flags rules that expose sensitive management services to the public internet.
/// Example from the spec: ALLOW 0.0.0.0/0 -> 10.0.10.15 TCP/22 => CRITICAL.
/// </summary>
public sealed class PublicExposureCheck : IPolicyRuleCheck
{
    private static readonly HashSet<int> DangerousPorts = [22, 23, 3389, 445, 135, 3306, 1433, 5432, 6379, 27017, 5985, 5986];

    public string Name => "PublicExposure";

    public IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var findings = new List<PolicyFinding>();

        foreach (var rule in rules)
        {
            if (rule.Action != RuleAction.Allow || !rule.Source.IsAny)
                continue;

            var exposesDangerousPort = DangerousPorts.Any(rule.Port.Contains);

            if (exposesDangerousPort)
            {
                findings.Add(new PolicyFinding(
                    FindingCode.PublicExposure,
                    FindingSeverity.Critical,
                    $"CRITICAL: Public exposure detected. Rule {rule.Priority} allows 0.0.0.0/0 to {rule.Destination} on sensitive port(s) within {rule.Port}.",
                    [rule.Id]));
            }
        }

        return findings;
    }
}