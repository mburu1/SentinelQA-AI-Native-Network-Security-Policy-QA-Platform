using SentinelQA.Domain.Common;
using SentinelQA.Domain.Entities;
using SentinelQA.Domain.Enums;
using SentinelQA.Domain.Events;
using SentinelQA.Domain.Exceptions;

namespace SentinelQA.Domain.Aggregates;

public sealed class Policy : AggregateRoot<Guid>
{
    private readonly List<PolicyRule> _rules = [];

    public Guid TenantId { get; private set; }
    public Guid FirewallId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Version { get; private set; }
    public PolicyStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<PolicyRule> Rules => _rules.AsReadOnly();

    private Policy() { } // EF Core

    public static Policy Create(Guid tenantId, Guid firewallId, string name, IEnumerable<PolicyRule> rules)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var policy = new Policy
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            FirewallId = firewallId,
            Name = name.Trim(),
            Version = 1,
            Status = PolicyStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        foreach (var rule in rules)
            policy.AddRule(rule);

        policy.AddDomainEvent(new PolicyCreated(policy.Id, tenantId, firewallId, policy.Name, policy.Version));
        return policy;
    }

    public void AddRule(PolicyRule rule)
    {
        if (_rules.Any(r => r.Priority == rule.Priority))
            throw new DomainException($"A rule with priority {rule.Priority} already exists in policy '{Name}'.");

        _rules.Add(rule);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReplaceRules(IEnumerable<PolicyRule> rules)
    {
        EnsureMutable();
        _rules.Clear();

        foreach (var rule in rules)
            AddRule(rule);

        Version++;
        Status = PolicyStatus.Draft;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new PolicyUpdated(Id, TenantId, Version));
    }

    public void RecordValidationPassed()
    {
        Status = PolicyStatus.Validated;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkDeployed()
    {
        Status = PolicyStatus.Deployed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureMutable()
    {
        if (Status == PolicyStatus.Deployed)
            throw new DomainException($"Policy '{Name}' is deployed and cannot be modified. Create a new change request instead.");
    }
}