using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Entities;

public sealed class PolicyValidationResult : Entity<Guid>
{
    public Guid PolicyId { get; private set; }
    public int PolicyVersion { get; private set; }
    public bool Passed { get; private set; }
    public int FindingsCount { get; private set; }
    public int CriticalCount { get; private set; }
    public string FindingsJson { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private PolicyValidationResult() { } // EF Core

    public PolicyValidationResult(Guid policyId, int policyVersion, bool passed, int findingsCount, int criticalCount, string findingsJson)
    {
        Id = Guid.CreateVersion7();
        PolicyId = policyId;
        PolicyVersion = policyVersion;
        Passed = passed;
        FindingsCount = findingsCount;
        CriticalCount = criticalCount;
        FindingsJson = findingsJson;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}