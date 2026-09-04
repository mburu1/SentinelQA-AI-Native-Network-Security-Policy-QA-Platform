using MediatR;

namespace SentinelQA.Modules.Identity.Features.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(Guid UserId, Guid TenantId, string AccessToken, string RefreshToken, IReadOnlyCollection<string> Roles);