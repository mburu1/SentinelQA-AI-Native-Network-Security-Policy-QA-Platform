using MediatR;

namespace SentinelQA.Modules.Firewalls.Features.RegisterFirewall;

public sealed record RegisterFirewallCommand(string Name, string Vendor, string Environment, string ConnectionType) : IRequest<Guid>;