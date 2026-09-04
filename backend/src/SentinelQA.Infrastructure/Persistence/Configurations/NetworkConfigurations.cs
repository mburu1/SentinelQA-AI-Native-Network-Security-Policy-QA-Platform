using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Infrastructure.Persistence.Configurations;

public sealed class FirewallConfiguration : IEntityTypeConfiguration<Firewall>
{
    public void Configure(EntityTypeBuilder<Firewall> builder)
    {
        builder.ToTable("firewalls", "network");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();
        builder.Property(f => f.Name).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Vendor).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Environment).HasMaxLength(50).IsRequired();
        builder.HasIndex(f => new { f.TenantId, f.Environment });
    }
}

public sealed class NetworkConfiguration : IEntityTypeConfiguration<Network>
{
    public void Configure(EntityTypeBuilder<Network> builder)
    {
        builder.ToTable("networks", "network");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.Name).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Cidr).HasMaxLength(64).IsRequired();
        builder.Property(n => n.Description).HasMaxLength(1000);
        builder.HasIndex(n => n.TenantId);
    }
}