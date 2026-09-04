namespace SentinelQA.Domain.Rules;

public sealed record PolicyFinding(FindingCode Code, FindingSeverity Severity, string Message, IReadOnlyList<Guid> RuleIds);