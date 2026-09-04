using Microsoft.Extensions.DependencyInjection;
using SentinelQA.Modules.Audit.EventHandlers;

namespace SentinelQA.Modules.Audit;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        services.AddScoped<AuditWriter>();
        return services;
    }
}