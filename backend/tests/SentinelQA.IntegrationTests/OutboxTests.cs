using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.ValueObjects;
using Xunit;

namespace SentinelQA.IntegrationTests;

[Collection("postgres")]
public sealed class OutboxTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Saving_policy_with_domain_event_writes_outbox_row()
    {
        await using var context = fixture.CreateContext();

        var policy = Policy.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "web-tier-ingress",
            [new PolicyRule(10, CidrBlock.Parse("10.0.0.0/24"), CidrBlock.Parse("10.1.0.0/24"),
                Protocol.Tcp, PortRange.Single(443), RuleAction.Allow, Direction.Inbound, true)]);

        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var outboxCount = await context.OutboxMessages
            .CountAsync(m => m.EventType == "PolicyCreatedIntegrationEvent");

        outboxCount.Should().Be(1);
    }
}