using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Rules.Checks;

namespace SentinelQA.Domain.Rules;

/// <summary>
/// Domain service orchestrating all policy rule checks.
/// Rules are always analyzed in priority order (firewall evaluation order).
/// </summary>
public sealed class PolicyAnalyzer
{
    private readonly IReadOnlyList<IPolicyRuleCheck> _checks;

    public PolicyAnalyzer() : this(DefaultChecks) { }

    public PolicyAnalyzer(IEnumerable<IPolicyRuleCheck> checks) => _checks = checks.ToList();

    public static IReadOnlyList<IPolicyRuleCheck> DefaultChecks { get; } =
    [
        new DuplicateRuleCheck(),
        new ShadowedRuleCheck(),
        new OverlapAndConflictCheck(),
        new PublicExposureCheck(),
        new OverlyBroadRuleCheck(),
        new MissingJustificationCheck()
    ];

    public PolicyAnalysisResult Analyze(IReadOnlyList<PolicyRule> rules)
    {
        var ordered = rules.OrderBy(r => r.Priority).ToList();

        var findings = _checks
            .SelectMany(check => check.Analyze(ordered))
            .OrderByDescending(f => f.Severity)
            .ToList();

        return new PolicyAnalysisResult(findings);
    }
}