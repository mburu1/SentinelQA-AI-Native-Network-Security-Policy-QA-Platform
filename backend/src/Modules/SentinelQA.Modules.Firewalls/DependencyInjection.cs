using Microsoft.Extensions.DependencyInjection;

namespace SentinelQA.Modules.Firewalls;

public static class DependencyInjection
{
    public static IServiceCollection AddFirewallsModule(this IServiceCollection services) => services;
}