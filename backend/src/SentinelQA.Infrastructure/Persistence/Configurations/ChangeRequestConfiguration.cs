using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;

namespace SentinelQA.Infrastructure.Persistence.Configurations;

public sealed class ChangeRequestConfiguration : IEntityTypeConfiguration<ChangeRequest>
{
    public void Configure(EntityTypeBuilder<ChangeRequest> builder)
    {
        builder.ToTable("change_requests", "change_management");

        builder.HasKey(cr => cr.Id);
        builder.Property(cr => cr.Id).ValueGeneratedNever();

        // Removed cr.Reason as it does not exist on the ChangeRequest aggregate
        builder.Property(cr => cr.RejectionReason).HasMaxLength(2000);
        builder.Property(cr => cr.DeploymentRef).HasMaxLength(128);

        // Explicit shadow property mapping bypasses the missing UseXminAsConcurrencyToken extension method error
        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .ValueGeneratedOnAddOrUpdate()
               .IsConcurrencyToken();

        // Changed cr.State to cr.Status to match the domain entity
        builder.HasIndex(cr => cr.Status);
        builder.HasIndex(cr => new { cr.TenantId, cr.PolicyId });
    }
}