using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Infrastructure.Configurations;

public sealed class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("milestones");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(m => m.DreamId);
        builder.UseXminConcurrencyToken();
    }
}
