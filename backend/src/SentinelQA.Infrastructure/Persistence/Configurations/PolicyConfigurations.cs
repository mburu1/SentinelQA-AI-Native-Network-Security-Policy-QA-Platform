using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;

namespace SentinelQA.Infrastructure.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policies", "policy");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        // PostgreSQL xmin system column used as the optimistic
        // concurrency token by Npgsql.
        builder.Property<uint>("xmin")
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasMany(p => p.Rules)
            .WithOne()
            .HasForeignKey("PolicyId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Policy.Rules))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new
        {
            p.TenantId,
            p.FirewallId
        });
    }
}


public sealed class PolicyRuleConfiguration
    : IEntityTypeConfiguration<PolicyRule>
{
    public void Configure(EntityTypeBuilder<PolicyRule> builder)
    {
        builder.ToTable("policy_rules", "policy");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        // Value objects persisted as canonical strings.
        builder.Property(r => r.Source)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(r => r.Destination)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(r => r.Port)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Justification)
            .HasMaxLength(2000);

        builder.HasIndex("PolicyId", nameof(PolicyRule.Priority))
            .IsUnique();
    }
}


public sealed class PolicyValidationResultConfiguration
    : IEntityTypeConfiguration<PolicyValidationResult>
{
    public void Configure(EntityTypeBuilder<PolicyValidationResult> builder)
    {
        builder.ToTable(
            "policy_validation_results",
            "policy");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.HasIndex(r => r.PolicyId);
    }
}