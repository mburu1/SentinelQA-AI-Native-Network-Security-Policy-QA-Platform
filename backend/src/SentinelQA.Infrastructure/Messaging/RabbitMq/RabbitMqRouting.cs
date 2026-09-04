using SentinelQA.Contracts.Messages;

namespace SentinelQA.Infrastructure.Messaging.RabbitMq;

/// <summary>Command type -> durable queue mapping (spec §10.1).</summary>
public static class RabbitMqRouting
{
    public const string Exchange = "sentinelqa.commands";

    private static readonly Dictionary<Type, string> CommandQueues = new()
    {
        [typeof(ValidateChangeRequestCommand)] = "sentinelqa.changemanagement",
        [typeof(RunTestSuiteCommand)] = "sentinelqa.testing",
        [typeof(AnalyzeFailureWithAiCommand)] = "sentinelqa.ai",
        [typeof(DeployFirewallPolicyCommand)] = "sentinelqa.deployments",
        [typeof(RollbackDeploymentCommand)] = "sentinelqa.deployments",
        [typeof(SendNotificationEmailCommand)] = "sentinelqa.notifications"
    };

    public static string QueueFor(Type commandType) =>
        CommandQueues.TryGetValue(commandType, out var queue)
            ? queue
            : throw new InvalidOperationException($"No queue registered for command '{commandType.Name}'.");

    public static IReadOnlyCollection<string> AllQueues => CommandQueues.Values.Distinct().ToList();

    public static string RoutingKeyFor(Type commandType) => commandType.Name;
}