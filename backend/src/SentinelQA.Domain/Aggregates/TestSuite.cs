using SentinelQA.Domain.Common;
using SentinelQA.Domain.Entities;

namespace SentinelQA.Domain.Aggregates;

public sealed class TestSuite : AggregateRoot<Guid>
{
    private readonly List<TestCase> _cases = [];

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsRegression { get; private set; }
    public IReadOnlyCollection<TestCase> Cases => _cases.AsReadOnly();

    private TestSuite() { } // EF Core

    public static TestSuite Define(Guid tenantId, string name, bool isRegression, IEnumerable<TestCase> cases)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var suite = new TestSuite
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Name = name.Trim(),
            IsRegression = isRegression
        };

        suite._cases.AddRange(cases);
        return suite;
    }
}