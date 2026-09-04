namespace SentinelQA.Infrastructure.Messaging.Kafka;

/// <summary>Event name -> Kafka topic mapping (spec §10.2).</summary>
public static class KafkaTopics
{
    public static string For(string eventType)
    {
        if (eventType.StartsWith("Policy", StringComparison.Ordinal))
            return "sentinelqa.policy-events";

        if (eventType.StartsWith("ChangeRequest", StringComparison.Ordinal) || eventType.StartsWith("Deployment", StringComparison.Ordinal))
            return "sentinelqa.change-events";

        if (eventType.StartsWith("TestRun", StringComparison.Ordinal) || eventType.StartsWith("Defect", StringComparison.Ordinal))
            return "sentinelqa.test-events";

        if (eventType.StartsWith("User", StringComparison.Ordinal))
            return "sentinelqa.audit-events";

        if (eventType.StartsWith("Notification", StringComparison.Ordinal))
            return "sentinelqa.notification-events";

        return "sentinelqa.audit-events";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        "sentinelqa.policy-events",
        "sentinelqa.change-events",
        "sentinelqa.test-events",
        "sentinelqa.security-events",
        "sentinelqa.audit-events",
        "sentinelqa.notification-events"
    ];
}