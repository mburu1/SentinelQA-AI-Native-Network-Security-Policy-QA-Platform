using FluentAssertions;
using SentinelQA.Contracts.Events;
using Xunit;

namespace SentinelQA.ContractTests;

/// <summary>Guards the public event schema. Breaking these properties breaks Kafka consumers.</summary>
public sealed class EventContractTests
{
    [Fact]
    public void Policy_created_event_stable_schema()
    {
        var @event = new PolicyCreatedIntegrationEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "policy", 1);

        @event.EventType.Should().Be("PolicyCreatedIntegrationEvent");
        @event.EventId.Should().NotBeEmpty();
        @event.OccurredOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}