namespace SentinelQA.Contracts.Messages;

public sealed record SendNotificationEmailCommand(string To, string TemplateKey, string Subject, Dictionary<string, string> Parameters, Guid TenantId) : CommandBase;