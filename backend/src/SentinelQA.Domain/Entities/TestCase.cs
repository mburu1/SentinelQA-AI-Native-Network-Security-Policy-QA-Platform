using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Entities;

public sealed class TestCase : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public TestCaseType Type { get; private set; }
    public string Steps { get; private set; } = default!;
    public string ExpectedResult { get; private set; } = default!;

    private TestCase() { } // EF Core

    public TestCase(string name, TestCaseType type, string steps, string expectedResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.CreateVersion7();
        Name = name.Trim();
        Type = type;
        Steps = steps;
        ExpectedResult = expectedResult;
    }
}