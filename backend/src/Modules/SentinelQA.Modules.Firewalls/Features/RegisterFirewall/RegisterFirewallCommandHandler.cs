using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.External;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Modules.Firewalls.Features.RegisterFirewall;

internal sealed class RegisterFirewallCommandHandler(
    IRepository<Firewall, Guid> firewalls,
    IUnitOfWork unitOfWork,
    IFirewallProvider provider,
    ICurrentUser currentUser) : IRequestHandler<RegisterFirewallCommand, Guid>
{
    public async Task<Guid> Handle(RegisterFirewallCommand request, CancellationToken cancellationToken)
    {
        var firewall = Firewall.Register(
            currentUser.TenantId,
            request.Name,
            request.Vendor,
            request.Environment,
            Enum.Parse<FirewallConnectionType>(request.ConnectionType, ignoreCase: true));

        var health = await provider.GetStatusAsync(firewall.Id, cancellationToken);
        firewall.RecordHealth(Enum.Parse<FirewallStatus>(health.Status, ignoreCase: true));

        await firewalls.AddAsync(firewall, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return firewall.Id;
    }
}