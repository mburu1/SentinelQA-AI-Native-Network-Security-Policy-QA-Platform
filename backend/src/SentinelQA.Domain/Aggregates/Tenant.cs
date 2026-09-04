using SentinelQA.Domain.Common;

namespace SentinelQA.Domain.Aggregates;

public sealed class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Tenant() { } // EF Core

    public static Tenant Provision(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Slug = new string(name.Trim().ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray()),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
}