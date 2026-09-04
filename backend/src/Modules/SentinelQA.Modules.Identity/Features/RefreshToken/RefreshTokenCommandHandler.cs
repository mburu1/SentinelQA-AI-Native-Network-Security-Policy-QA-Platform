using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;

namespace SentinelQA.Modules.Identity.Features.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ITokenService tokens,
    IClock clock) : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var presentedHash = tokens.HashToken(request.RefreshToken);
        var user = await users.GetByRefreshTokenHashAsync(presentedHash, cancellationToken)
            ?? throw new NotFoundException("Invalid refresh token.");

        if (!user.HasValidRefreshToken(presentedHash, clock.UtcNow))
            throw new ConflictException("Refresh token has expired.");

        // Refresh-token rotation.
        var newRefreshToken = tokens.CreateRefreshToken();
        user.SetRefreshToken(tokens.HashToken(newRefreshToken), clock.UtcNow.AddDays(14));

        var accessToken = tokens.CreateAccessToken(user.Id, user.TenantId, user.Email, user.Roles);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(accessToken, newRefreshToken);
    }
}