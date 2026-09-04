using SentinelQA.Contracts.Events;
using SentinelQA.Contracts.Messaging;

namespace SentinelQA.Infrastructure.Persistence.Outbox;

public static class IntegrationEventRegistry
{
    private static readonly Dictionary<string, Type> ByName = new[]
    {
        typeof(PolicyCreatedIntegrationEvent),
        typeof(PolicyValidatedIntegrationEvent),
        typeof(ChangeRequestSubmittedIntegrationEvent),
        typeof(ChangeRequestApprovedIntegrationEvent),
        typeof(DeploymentCompletedIntegrationEvent),
        typeof(TestRunCompletedIntegrationEvent),
        typeof(DefectCreatedIntegrationEvent)
    }.ToDictionary(t => t.Name, t => t);

    public static Type? Resolve(string eventType) => ByName.GetValueOrDefault(eventType);
}