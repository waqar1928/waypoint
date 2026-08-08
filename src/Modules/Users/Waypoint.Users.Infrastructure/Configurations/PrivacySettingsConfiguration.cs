using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Users.Domain;

namespace Waypoint.Users.Infrastructure.Configurations;

public sealed class PrivacySettingsConfiguration : IEntityTypeConfiguration<PrivacySettings>
{
    public void Configure(EntityTypeBuilder<PrivacySettings> builder)
    {
        builder.ToTable("users_privacy_settings");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ProfileVisibility).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.DreamVisibility).HasConversion<string>().HasMaxLength(20);
        builder.UseXminConcurrencyToken();
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
