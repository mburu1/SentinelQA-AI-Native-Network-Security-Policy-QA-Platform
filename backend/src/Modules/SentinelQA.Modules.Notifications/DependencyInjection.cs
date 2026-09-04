using Microsoft.Extensions.DependencyInjection;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Modules.Notifications.EventHandlers;

namespace SentinelQA.Modules.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        services.AddScoped<Application.Abstractions.Messaging.ICommandHandler, Workers.SendNotificationEmailCommandHandler>();
        services.AddScoped<IChangeRequestOwnerLookup, ChangeRequestOwnerLookup>();
        return services;
    }

    private sealed class ChangeRequestOwnerLookup(IChangeRequestRepository repository) : IChangeRequestOwnerLookup
    {
        public async Task<Guid?> GetRequesterAsync(Guid changeRequestId, CancellationToken cancellationToken) =>
            (await repository.GetByIdAsync(changeRequestId, cancellationToken))?.RequestedBy;
    }
}