using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Rules.Checks;

public sealed class MissingJustificationCheck : IPolicyRuleCheck
{
    public string Name => "MissingJustification";

    public IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var findings = new List<PolicyFinding>();

        foreach (var rule in rules)
        {
            var requiresJustification = rule.Action == RuleAction.Allow && (rule.Source.IsAny || rule.Port.IsAny);

            if (requiresJustification && string.IsNullOrWhiteSpace(rule.Justification))
            {
                findings.Add(new PolicyFinding(
                    FindingCode.MissingJustification,
                    FindingSeverity.Info,
                    $"Rule {rule.Priority} is broad and should include a business justification for audit purposes.",
                    [rule.Id]));
            }
        }

        return findings;
    }
}