using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Behaviors;
using SentinelQA.Application.Infrastructure;
using SentinelQA.Domain.Rules;

namespace SentinelQA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] moduleAssemblies)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<PolicyAnalyzer>();

        var assemblies = new[] { typeof(DependencyInjection).Assembly }.Concat(moduleAssemblies).ToArray();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        foreach (var assembly in assemblies)
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}