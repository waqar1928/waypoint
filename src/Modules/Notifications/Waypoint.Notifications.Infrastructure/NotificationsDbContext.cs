using Microsoft.EntityFrameworkCore;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Infrastructure;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Body).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.LinkUrl).HasMaxLength(500);

            // GetForUserAsync does `WHERE recipient_user_id = @p ORDER BY created_at DESC LIMIT n`
            // and GetUnreadCountAsync does `WHERE recipient_user_id = @p AND is_read = false` — this
            // composite index serves both without a second index, since Postgres can use a leading
            // prefix of a composite index for the count query too.
            entity.HasIndex(e => new { e.RecipientUserId, e.IsRead, e.CreatedAt });
        });
    }
}
