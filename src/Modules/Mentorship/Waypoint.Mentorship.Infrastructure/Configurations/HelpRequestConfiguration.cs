using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Infrastructure.Configurations;

public sealed class HelpRequestConfiguration : IEntityTypeConfiguration<HelpRequest>
{
    public void Configure(EntityTypeBuilder<HelpRequest> builder)
    {
        builder.ToTable("help_requests");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.Title).HasMaxLength(200).IsRequired();
        builder.Property(h => h.Body).HasMaxLength(2000).IsRequired();
        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => new { h.Category, h.Status });

        builder.UseXminConcurrencyToken();
    }
}
