using SentinelQA.Domain.Entities;

namespace SentinelQA.Domain.Rules;

public interface IPolicyRuleCheck
{
    string Name { get; }
    IReadOnlyList<PolicyFinding> Analyze(IReadOnlyList<PolicyRule> rules);
}