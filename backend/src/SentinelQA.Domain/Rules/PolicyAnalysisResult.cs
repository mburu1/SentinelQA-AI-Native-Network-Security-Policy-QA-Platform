namespace SentinelQA.Domain.Rules;

public sealed record PolicyAnalysisResult(IReadOnlyList<PolicyFinding> Findings)
{
    public bool IsValid => !Findings.Any(f => f.Severity is FindingSeverity.Error or FindingSeverity.Critical);
    public bool HasCritical => Findings.Any(f => f.Severity == FindingSeverity.Critical);

    public int CountBySeverity(FindingSeverity severity) => Findings.Count(f => f.Severity == severity);
}