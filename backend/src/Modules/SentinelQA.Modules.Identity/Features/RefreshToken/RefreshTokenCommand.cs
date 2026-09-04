using MediatR;

namespace SentinelQA.Modules.Identity.Features.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;

public sealed record RefreshTokenResult(string AccessToken, string RefreshToken);