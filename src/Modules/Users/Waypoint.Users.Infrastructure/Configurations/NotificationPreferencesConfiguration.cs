using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Users.Domain;

namespace Waypoint.Users.Infrastructure.Configurations;

public sealed class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.ToTable("users_notification_preferences");
        builder.HasKey(p => p.Id);
        builder.UseXminConcurrencyToken();
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}
