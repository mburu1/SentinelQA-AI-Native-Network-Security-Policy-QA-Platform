using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Common;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.ValueObjects;
using SentinelQA.Infrastructure.Common;
using SentinelQA.Infrastructure.Persistence.Outbox;

namespace SentinelQA.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator, IntegrationEventMapper eventMapper)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Firewall> Firewalls => Set<Firewall>();
    public DbSet<Network> Networks => Set<Network>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyValidationResult> PolicyValidationResults => Set<PolicyValidationResult>();
    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<TestSuite> TestSuites => Set<TestSuite>();
    public DbSet<TestRun> TestRuns => Set<TestRun>();
    public DbSet<Defect> Defects => Set<Defect>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<CidrBlock>().HaveConversion<CidrBlockConverter>();
        configurationBuilder.Properties<PortRange>().HaveConversion<PortRangeConverter>();

        configurationBuilder.Properties<Protocol>().HaveConversion<string>();
        configurationBuilder.Properties<RuleAction>().HaveConversion<string>();
        configurationBuilder.Properties<Direction>().HaveConversion<string>();
        configurationBuilder.Properties<PolicyStatus>().HaveConversion<string>();
        configurationBuilder.Properties<FirewallStatus>().HaveConversion<string>();
        configurationBuilder.Properties<FirewallConnectionType>().HaveConversion<string>();
        configurationBuilder.Properties<NetworkType>().HaveConversion<string>();
        configurationBuilder.Properties<ChangeRequestState>().HaveConversion<string>();
        configurationBuilder.Properties<TestRunStatus>().HaveConversion<string>();
        configurationBuilder.Properties<TestResultStatus>().HaveConversion<string>();
        configurationBuilder.Properties<TestCaseType>().HaveConversion<string>();
        configurationBuilder.Properties<DefectStatus>().HaveConversion<string>();
        configurationBuilder.Properties<Severity>().HaveConversion<string>();
        configurationBuilder.Properties<PriorityLevel>().HaveConversion<string>();
        configurationBuilder.Properties<NotificationStatus>().HaveConversion<string>();
        configurationBuilder.Properties<NotificationChannel>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();
        EnqueueOutboxMessages(domainEvents);

        var result = await base.SaveChangesAsync(cancellationToken);

        // In-process handlers (audit, notifications, cross-module reactions) run after the transaction commits.
        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent, cancellationToken);

        return result;
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
        aggregates.ForEach(a => a.ClearDomainEvents());
        return events;
    }

    private void EnqueueOutboxMessages(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var integrationEvent = eventMapper.Map(domainEvent);
            if (integrationEvent is null)
                continue;

            OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                EventType = integrationEvent.EventType,
                Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonDefaults.Options),
                OccurredOn = integrationEvent.OccurredOn,
                CorrelationId = integrationEvent.CorrelationId
            });
        }
    }
}