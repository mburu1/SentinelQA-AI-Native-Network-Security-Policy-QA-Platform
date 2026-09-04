using MediatR;

namespace SentinelQA.Modules.Tenants.Features.CreateTenant;

public sealed record CreateTenantCommand(string Name) : IRequest<Guid>;

internal sealed class CreateTenantCommandHandler(
    Application.Abstractions.Persistence.IRepository<Domain.Aggregates.Tenant, Guid> tenants,
    Application.Abstractions.IUnitOfWork unitOfWork) : IRequestHandler<CreateTenantCommand, Guid>
{
    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = Domain.Aggregates.Tenant.Provision(request.Name);
        await tenants.AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return tenant.Id;
    }
}