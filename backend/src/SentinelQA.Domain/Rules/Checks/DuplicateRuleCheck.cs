using SentinelQA.Domain.Entities;

namespace SentinelQA.Domain.Rules.Checks;

public sealed class DuplicateRuleCheck : IPolicyRuleCheck
{
    public string Name => "DuplicateRule";

    public IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var findings = new List<PolicyFinding>();

        var groups = rules.GroupBy(r => (r.Source, r.Destination, r.Protocol, r.Port, r.Action, r.Direction));

        foreach (var group in groups.Where(g => g.Count() > 1))
        {
            var ids = group.Select(r => r.Id).ToList();
            findings.Add(new PolicyFinding(
                FindingCode.DuplicateRule,
                FindingSeverity.Warning,
                $"Duplicate rule detected: {group.First().Source} -> {group.First().Destination} {group.First().Protocol}/{group.First().Port} {group.First().Action}.",
                ids));
        }

        return findings;
    }
}