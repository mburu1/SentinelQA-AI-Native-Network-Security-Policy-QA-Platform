using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Modules.Identity.Features.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Parse(request.Email);

        if (await users.GetByEmailAsync(email.Value, cancellationToken) is not null)
            throw new ConflictException($"A user with email '{email.Value}' already exists.");

        var user = User.Register(request.TenantId, email, request.DisplayName, passwordHasher.Hash(request.Password), request.Roles);

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}