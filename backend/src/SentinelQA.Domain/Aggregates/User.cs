using SentinelQA.Domain.Common;
using SentinelQA.Domain.Exceptions;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Domain.Aggregates;

public sealed class User : AggregateRoot<Guid>
{
    public List<string> Roles { get; private set; } = [];

    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }

    private User() { } // EF Core

    public static User Register(Guid tenantId, Email email, string displayName, string passwordHash, IEnumerable<string> roles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Email = email.Value,
            DisplayName = displayName.Trim(),
            PasswordHash = passwordHash,
            IsActive = true,
            Roles = roles.ToList()
        };
    }

    public bool HasValidRefreshToken(string presentedHash, DateTimeOffset now) =>
        RefreshTokenHash is not null
        && RefreshTokenExpiresAt > now
        && string.Equals(RefreshTokenHash, presentedHash, StringComparison.Ordinal);

    public void SetRefreshToken(string hash, DateTimeOffset expiresAt)
    {
        RefreshTokenHash = hash;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
    }

    public void SetRoles(IEnumerable<string> roles)
    {
        var list = roles.ToList();
        if (list.Count == 0)
            throw new DomainException("A user must have at least one role.");

        Roles = list;
    }

    public void Deactivate() => IsActive = false;
}