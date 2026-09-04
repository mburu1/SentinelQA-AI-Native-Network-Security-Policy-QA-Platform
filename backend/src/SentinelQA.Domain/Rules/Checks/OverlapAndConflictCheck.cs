using SentinelQA.Domain.Entities;

namespace SentinelQA.Domain.Rules.Checks;

/// <summary>
/// Detects partially overlapping rules. Overlaps with different actions are flagged
/// as conflicts because rule ordering now determines the security outcome.
/// </summary>
public sealed class OverlapAndConflictCheck : IPolicyRuleCheck
{
    public string Name => "OverlapAndConflict";

    public IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var findings = new List<PolicyFinding>();
        var ordered = rules.OrderBy(r => r.Priority).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            for (var j = i + 1; j < ordered.Count; j++)
            {
                var a = ordered[i];
                var b = ordered[j];

                if (!a.ScopesOverlap(b) || a.ScopeContains(b) || b.ScopeContains(a))
                    continue;

                findings.Add(a.Action != b.Action
                    ? new PolicyFinding(
                        FindingCode.ConflictingActions,
                        FindingSeverity.Warning,
                        $"Rules {a.Priority} and {b.Priority} overlap with conflicting actions. Order determines the outcome.",
                        [a.Id, b.Id])
                    : new PolicyFinding(
                        FindingCode.OverlappingRules,
                        FindingSeverity.Info,
                        $"Rules {a.Priority} and {b.Priority} partially overlap. Consider consolidating.",
                        [a.Id, b.Id]));
            }
        }

        return findings;
    }
}