using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelQA.Domain.Aggregates;

namespace SentinelQA.Infrastructure.Persistence.Configurations;

public sealed class ChangeRequestConfiguration : IEntityTypeConfiguration<ChangeRequest>
{
    public void Configure(EntityTypeBuilder<ChangeRequest> builder)
    {
        builder.ToTable("change_requests", "change_management");

        builder.HasKey(cr => cr.Id);
        builder.Property(cr => cr.Id).ValueGeneratedNever();
        builder.Property(cr => cr.Reason).HasMaxLength(2000).IsRequired();
        builder.Property(cr => cr.RejectionReason).HasMaxLength(2000);
        builder.Property(cr => cr.DeploymentRef).HasMaxLength(128);
        builder.UseXminAsConcurrencyToken();

        builder.HasIndex(cr => cr.State);
        builder.HasIndex(cr => new { cr.TenantId, cr.PolicyId });
    }
}