using SentinelQA.Application.Abstractions.Infrastructure;
using SentinelQA.Application.Abstractions.Messaging;
using SentinelQA.Contracts.Messages;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Modules.Notifications.Workers;

internal sealed class SendNotificationEmailCommandHandler(
    IEmailSender email,
    Application.Abstractions.Persistence.IRepository<NotificationDelivery, Guid> deliveries,
    Application.Abstractions.IUnitOfWork unitOfWork) : CommandHandlerBase<SendNotificationEmailCommand>
{
    public override async Task HandleAsync(SendNotificationEmailCommand command, CancellationToken cancellationToken)
    {
        var delivery = NotificationDelivery.Queue(command.TenantId, command.To, command.TemplateKey, command.Subject);
        delivery.RegisterAttempt();

        try
        {
            var body = EmailTemplates.Render(command.TemplateKey, command.Parameters);
            await email.SendAsync(command.To, command.Subject, body, cancellationToken);
            delivery.MarkSent();
        }
        catch (Exception ex)
        {
            delivery.MarkFailed(ex.Message);
        }

        await deliveries.AddAsync(delivery, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal static class EmailTemplates
{
    public static string Render(string templateKey, IReadOnlyDictionary<string, string> parameters)
    {
        var body = templateKey switch
        {
            "change-request-submitted" => $"<p>Change request <b>{parameters.GetValueOrDefault("changeRequestId")}</b> was submitted and is now being validated.</p>",
            "change-request-approved" => $"<p>Change request <b>{parameters.GetValueOrDefault("changeRequestId")}</b> has been approved.</p>",
            "deployment-completed" => $"<p>Deployment finished. Success: <b>{parameters.GetValueOrDefault("success")}</b>.</p>",
            _ => "<p>SentinelQA notification.</p>"
        };

        return $"<html><body><h3>SentinelQA</h3>{body}</body></html>";
    }
}