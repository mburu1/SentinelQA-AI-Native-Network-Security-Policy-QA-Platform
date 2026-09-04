using SentinelQA.Domain.Entities;

namespace SentinelQA.Domain.Rules.Checks;

/// <summary>
/// A rule is shadowed when an earlier (lower priority) rule's scope fully contains it,
/// meaning the later rule can never match any traffic.
/// </summary>
public sealed class ShadowedRuleCheck : IPolicyRuleCheck
{
    public string Name => "ShadowedRule";

    public IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var findings = new List<PolicyFinding>();
        var ordered = rules.OrderBy(r => r.Priority).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            for (var j = i + 1; j < ordered.Count; j++)
            {
                var earlier = ordered[i];
                var later = ordered[j];

                if (!earlier.ScopeContains(later))
                    continue;

                var severity = earlier.Action == later.Action ? FindingSeverity.Warning : FindingSeverity.Error;
                var code = earlier.Action == later.Action ? FindingCode.ShadowedRule : FindingCode.UnreachableRule;

                findings.Add(new PolicyFinding(
                    code,
                    severity,
                    earlier.Action == later.Action
                        ? $"Rule {later.Priority} is redundant: fully shadowed by rule {earlier.Priority} with the same action."
                        : $"Rule {later.Priority} is unreachable: rule {earlier.Priority} matches all of its traffic first with the opposite action.",
                    [earlier.Id, later.Id]));
            }
        }

        return findings;
    }
}