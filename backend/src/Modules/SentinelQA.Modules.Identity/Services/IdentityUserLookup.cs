using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Application.Abstractions.Persistence;

namespace SentinelQA.Modules.Identity.Services;

/// <summary>Cross-module contract implementation so other modules never query identity tables directly.</summary>
internal sealed class IdentityUserLookup(IUserRepository users) : IUserLookup
{
    public async Task<string?> GetEmailAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await users.GetByIdAsync(userId, cancellationToken))?.Email;
}