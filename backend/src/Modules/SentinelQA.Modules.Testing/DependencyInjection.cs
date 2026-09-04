using Microsoft.Extensions.DependencyInjection;

namespace SentinelQA.Modules.Testing;

public static class DependencyInjection
{
    public static IServiceCollection AddTestingModule(this IServiceCollection services)
    {
        services.AddScoped<Application.Abstractions.Messaging.ICommandHandler, Workers.RunTestSuiteCommandHandler>();
        services.AddScoped<Application.Abstractions.Messaging.ICommandHandler, Workers.AnalyzeFailureWithAiCommandHandler>();
        return services;
    }
}