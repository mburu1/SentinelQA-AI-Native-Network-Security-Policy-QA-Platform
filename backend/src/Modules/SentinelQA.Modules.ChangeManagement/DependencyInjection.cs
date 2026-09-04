using Microsoft.Extensions.DependencyInjection;

namespace SentinelQA.Modules.ChangeManagement;

public static class DependencyInjection
{
    public static IServiceCollection AddChangeManagementModule(this IServiceCollection services)
    {
        // Command handlers are discovered via the ICommandHandler registration below.
        services.AddScoped<Application.Abstractions.Messaging.ICommandHandler, Workers.ValidateChangeRequestCommandHandler>();
        services.AddScoped<Application.Abstractions.Messaging.ICommandHandler, Workers.DeployFirewallPolicyCommandHandler>();
        return services;
    }
}