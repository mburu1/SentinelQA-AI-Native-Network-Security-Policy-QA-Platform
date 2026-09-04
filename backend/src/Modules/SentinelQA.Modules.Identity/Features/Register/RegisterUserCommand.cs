using MediatR;

namespace SentinelQA.Modules.Identity.Features.Register;

public sealed record RegisterUserCommand(Guid TenantId, string Email, string DisplayName, string Password, List<string> Roles) : IRequest<Guid>;