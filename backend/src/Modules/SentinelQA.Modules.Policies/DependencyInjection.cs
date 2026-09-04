using Microsoft.Extensions.DependencyInjection;
using SentinelQA.Infrastructure.Persistence;
using SentinelQA.Modules.Policies.Abstractions;

namespace SentinelQA.Modules.Policies;

public static class DependencyInjection
{
    public static IServiceCollection AddPoliciesModule(this IServiceCollection services)
    {
        services.AddScoped<IValidationResultStore, ValidationResultStore>();
        return services;
    }

    private sealed class ValidationResultStore(ApplicationDbContext context) : IValidationResultStore
    {
        public Task AddAsync(Domain.Entities.PolicyValidationResult result, CancellationToken cancellationToken) =>
            context.PolicyValidationResults.AddAsync(result, cancellationToken).AsTask();
    }
}