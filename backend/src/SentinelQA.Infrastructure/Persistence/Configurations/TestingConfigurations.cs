using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Entities;

namespace SentinelQA.Infrastructure.Persistence.Configurations;

public sealed class TestSuiteConfiguration : IEntityTypeConfiguration<TestSuite>
{
    public void Configure(EntityTypeBuilder<TestSuite> builder)
    {
        builder.ToTable("test_suites", "qa");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();

        builder.HasMany(s => s.Cases)
            .WithOne()
            .HasForeignKey("TestSuiteId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(TestSuite.Cases))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class TestCaseConfiguration : IEntityTypeConfiguration<TestCase>
{
    public void Configure(EntityTypeBuilder<TestCase> builder)
    {
        builder.ToTable("test_cases", "qa");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Steps).IsRequired();
        builder.Property(c => c.ExpectedResult).IsRequired();
    }
}

public sealed class TestRunConfiguration : IEntityTypeConfiguration<TestRun>
{
    public void Configure(EntityTypeBuilder<TestRun> builder)
    {
        builder.ToTable("test_runs", "qa");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Environment).HasMaxLength(50).IsRequired();

        builder.HasMany(r => r.Results)
            .WithOne()
            .HasForeignKey("TestRunId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(TestRun.Results))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => r.Status);
    }
}

public sealed class TestResultConfiguration : IEntityTypeConfiguration<TestResult>
{
    public void Configure(EntityTypeBuilder<TestResult> builder)
    {
        builder.ToTable("test_results", "qa");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.CaseName).HasMaxLength(300).IsRequired();
        builder.Property(r => r.FailureMessage).HasMaxLength(4000);
    }
}