using System.Security.Claims;
using SentinelQA.Application.Abstractions;

namespace SentinelQA.Api.Infrastructure;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid UserId => ParseGuidClaim(ClaimTypes.NameIdentifier) ?? Guid.Empty;
    public Guid TenantId => ParseGuidClaim("tenant_id") ?? Guid.Empty;
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public IReadOnlyCollection<string> Roles => User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    private Guid? ParseGuidClaim(string type) =>
        Guid.TryParse(User?.FindFirstValue(type), out var value) ? value : null;
}