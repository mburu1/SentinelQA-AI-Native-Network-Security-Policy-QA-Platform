using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Identity;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Events;

namespace SentinelQA.Modules.Notifications.EventHandlers;

internal sealed class NotifyOnChangeRequestSubmitted(
    IUserLookup userLookup,
    ICommandPublisher commands,
    ICorrelationContext correlation) : INotificationHandler<ChangeRequestSubmitted>
{
    public async Task Handle(ChangeRequestSubmitted notification, CancellationToken cancellationToken)
    {
        var email = await userLookup.GetEmailAsync(notification.RequestedBy, cancellationToken);
        if (email is null)
            return;

        await commands.PublishAsync(new SendNotificationEmailCommand(
            email,
            "change-request-submitted",
            $"[SentinelQA] Change request submitted",
            new Dictionary<string, string> { ["changeRequestId"] = notification.ChangeRequestId.ToString() },
            notification.TenantId)
        {
            CorrelationId = correlation.CorrelationId
        }, cancellationToken);
    }
}

internal sealed class NotifyOnDeploymentCompleted(
    IChangeRequestOwnerLookup changeRequests,
    IUserLookup userLookup,
    ICommandPublisher commands,
    ICorrelationContext correlation) : INotificationHandler<DeploymentCompleted>
{
    public async Task Handle(DeploymentCompleted notification, CancellationToken cancellationToken)
    {
        var requester = await changeRequests.GetRequesterAsync(notification.ChangeRequestId, cancellationToken);
        if (requester is null)
            return;

        var email = await userLookup.GetEmailAsync(requester.Value, cancellationToken);
        if (email is null)
            return;

        await commands.PublishAsync(new SendNotificationEmailCommand(
            email,
            "deployment-completed",
            $"[SentinelQA] Deployment {(notification.Success ? "succeeded" : "failed")}",
            new Dictionary<string, string> { ["success"] = notification.Success.ToString() },
            notification.TenantId)
        {
            CorrelationId = correlation.CorrelationId
        }, cancellationToken);
    }
}

/// <summary>Small lookup abstraction so notifications never touch change-management persistence directly.</summary>
public interface IChangeRequestOwnerLookup
{
    Task<Guid?> GetRequesterAsync(Guid changeRequestId, CancellationToken cancellationToken);
}