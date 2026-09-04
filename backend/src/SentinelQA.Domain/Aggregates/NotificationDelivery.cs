using SentinelQA.Domain.Common;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Domain.Aggregates;

public sealed class NotificationDelivery : AggregateRoot<Guid>
{
    public Guid TenantId { get; private set; }
    public string Recipient { get; private set; } = default!;
    public NotificationChannel Channel { get; private set; }
    public string TemplateKey { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public NotificationStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private NotificationDelivery() { } // EF Core

    public static NotificationDelivery Queue(Guid tenantId, string recipient, string templateKey, string subject) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            Recipient = recipient,
            Channel = NotificationChannel.Email,
            TemplateKey = templateKey,
            Subject = subject,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

    public void RegisterAttempt() => Attempts++;
    public void MarkSent() { Status = NotificationStatus.Sent; LastError = null; }
    public void MarkFailed(string error) { Status = NotificationStatus.Failed; LastError = error; }
}