using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Infrastructure.Configurations;

public sealed class MentorProfileConfiguration : IEntityTypeConfiguration<MentorProfile>
{
    public void Configure(EntityTypeBuilder<MentorProfile> builder)
    {
        builder.ToTable("mentor_profiles");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.UserId).IsUnique();

        builder.Property(m => m.Expertise)
            .HasColumnType("jsonb")
            .HasConversion(JsonStringListConverter.Converter, JsonStringListConverter.Comparer);

        builder.Property(m => m.Availability).HasMaxLength(50);
        builder.Property(m => m.VerificationStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.RatingAvg).HasColumnType("numeric(3,2)");

        builder.UseXminConcurrencyToken();
    }
}
