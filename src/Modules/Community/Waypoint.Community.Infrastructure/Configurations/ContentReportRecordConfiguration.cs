using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Infrastructure.Configurations;

public sealed class ContentReportRecordConfiguration : IEntityTypeConfiguration<ContentReportRecord>
{
    public void Configure(EntityTypeBuilder<ContentReportRecord> builder)
    {
        builder.ToTable("content_reports");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.EntityType).HasMaxLength(30).IsRequired();
        builder.Property(r => r.Reason).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Details).HasMaxLength(1000);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(r => new { r.EntityType, r.EntityId });
        builder.HasIndex(r => r.Status);
        builder.UseXminConcurrencyToken();
    }
}
