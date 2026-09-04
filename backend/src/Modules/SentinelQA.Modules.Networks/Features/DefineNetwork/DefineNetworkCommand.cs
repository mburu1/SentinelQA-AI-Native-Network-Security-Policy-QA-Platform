using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Modules.Networks.Features.DefineNetwork;

public sealed record DefineNetworkCommand(string Name, string Cidr, string Type, string? Description) : IRequest<Guid>;

internal sealed class DefineNetworkCommandHandler(
    IRepository<Network, Guid> networks,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<DefineNetworkCommand, Guid>
{
    public async Task<Guid> Handle(DefineNetworkCommand request, CancellationToken cancellationToken)
    {
        var network = Network.Define(
            currentUser.TenantId,
            request.Name,
            CidrBlock.Parse(request.Cidr),
            Enum.Parse<NetworkType>(request.Type, ignoreCase: true),
            request.Description);

        await networks.AddAsync(network, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return network.Id;
    }
}