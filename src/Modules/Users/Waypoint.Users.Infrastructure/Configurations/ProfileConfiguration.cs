using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Users.Domain;

namespace Waypoint.Users.Infrastructure.Configurations;

public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("users_profile");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Bio).HasMaxLength(500);
        builder.Property(p => p.AvatarUrl).HasMaxLength(2048);
        builder.Property(p => p.TimeZone).HasMaxLength(64).IsRequired();
        builder.Property(p => p.Locale).HasMaxLength(16).IsRequired();
        builder.UseXminConcurrencyToken();
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
