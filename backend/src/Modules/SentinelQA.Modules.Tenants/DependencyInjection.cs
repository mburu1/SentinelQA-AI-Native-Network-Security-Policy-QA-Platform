using Microsoft.Extensions.DependencyInjection;

namespace SentinelQA.Modules.Tenants;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantsModule(this IServiceCollection services) => services;
}