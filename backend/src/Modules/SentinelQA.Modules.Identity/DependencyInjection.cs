using Microsoft.Extensions.DependencyInjection;
using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Modules.Identity.Services;

namespace SentinelQA.Modules.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddScoped<IUserLookup, IdentityUserLookup>();
        return services;
    }
}