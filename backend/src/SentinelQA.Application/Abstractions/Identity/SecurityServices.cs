namespace SentinelQA.Application.Abstractions.Identity;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string storedHash);
}

public interface ITokenService
{
    string CreateAccessToken(Guid userId, Guid tenantId, string email, IReadOnlyCollection<string> roles);
    string CreateRefreshToken();
    string HashToken(string token);
}

/// <summary>Cross-module lookup implemented by the Identity module.</summary>
public interface IUserLookup
{
    Task<string?> GetEmailAsync(Guid userId, CancellationToken cancellationToken = default);
}