using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Modules.Identity.Features.Login;

internal sealed class LoginCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokens,
    IClock clock) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Parse(request.Email);
        var user = await users.GetByEmailAsync(email.Value, cancellationToken)
            ?? throw new NotFoundException("Invalid credentials.");

        if (!user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new NotFoundException("Invalid credentials.");

        var accessToken = tokens.CreateAccessToken(user.Id, user.TenantId, user.Email, user.Roles);
        var refreshToken = tokens.CreateRefreshToken();

        user.SetRefreshToken(tokens.HashToken(refreshToken), clock.UtcNow.AddDays(14));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(user.Id, user.TenantId, accessToken, refreshToken, user.Roles);
    }
}