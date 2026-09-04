using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;
using SentinelQA.Infrastructure.Persistence.Outbox;

namespace SentinelQA.Infrastructure.Persistence.Configurations;

public sealed class DefectConfiguration : IEntityTypeConfiguration<Defect>
{
    public void Configure(EntityTypeBuilder<Defect> builder)
    {
        builder.ToTable("defects", "defects");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property(d => d.Title).HasMaxLength(300).IsRequired();
        builder.Property(d => d.Environment).HasMaxLength(50).IsRequired();

        builder.HasMany(d => d.History)
            .WithOne()
            .HasForeignKey("DefectId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Defect.History))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(d => d.Status);
    }
}

public sealed class DefectHistoryEntryConfiguration : IEntityTypeConfiguration<DefectHistoryEntry>
{
    public void Configure(EntityTypeBuilder<DefectHistoryEntry> builder)
    {
        builder.ToTable("defect_history", "defects");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();
        builder.Property(h => h.Note).HasMaxLength(2000);
    }
}

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries", "audit");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(64);

        builder.HasIndex(a => new { a.TenantId, a.OccurredAt });
    }
}

public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries", "notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.Recipient).HasMaxLength(320).IsRequired();
        builder.Property(n => n.TemplateKey).HasMaxLength(100).IsRequired();
        builder.Property(n => n.Subject).HasMaxLength(300).IsRequired();
        builder.Property(n => n.LastError).HasMaxLength(2000);
    }
}

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.EventType).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Error).HasMaxLength(4000);
        builder.HasIndex(o => new { o.ProcessedAt, o.OccurredOn });
    }
}