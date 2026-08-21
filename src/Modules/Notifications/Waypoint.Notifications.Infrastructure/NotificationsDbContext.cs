using Microsoft.EntityFrameworkCore;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Infrastructure;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<NotificationDeliveryHistory> DeliveryHistory => Set<NotificationDeliveryHistory>();

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

        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.ToTable("notifications_push_subscriptions");
            entity.HasKey(e => e.Id);
            // No FK to a Users table — cross-module boundary, same reasoning as
            // Notification.RecipientUserId: just an indexed Guid, resolved via
            // IProfileSummaryProvider/IPushReminderAudienceProvider when a display name or
            // timezone is actually needed.
            entity.Property(e => e.Endpoint).HasMaxLength(2048).IsRequired();
            entity.Property(e => e.P256dhKey).HasMaxLength(255).IsRequired();
            entity.Property(e => e.AuthKey).HasMaxLength(255).IsRequired();
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeactivatedReason).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            // The endpoint IS the device identity - re-subscribing the same browser upserts this
            // row rather than creating a duplicate (see PushSubscriptionRepository.UpsertAsync).
            entity.HasIndex(e => e.Endpoint).IsUnique();
            // Serves "get this user's active devices" (the worker's per-user fan-out) without a
            // second index.
            entity.HasIndex(e => new { e.UserId, e.Status });
        });

        modelBuilder.Entity<NotificationDeliveryHistory>(entity =>
        {
            entity.ToTable("notifications_delivery_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReminderKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            // THE load-bearing constraint for the whole idempotency design - see the class doc
            // comment on NotificationDeliveryHistory. A worker running twice, two overlapping app
            // instances, or a retry after a crash can all attempt to claim the same logical
            // reminder; only one row for a given (user, key) pair can ever exist.
            entity.HasIndex(e => new { e.UserId, e.ReminderKey }).IsUnique();
            // Serves the "stale Attempted rows" sweep (SELECT ... FOR UPDATE SKIP LOCKED) and the
            // daily-rate-limit count query, both of which filter by Status and a time range.
            entity.HasIndex(e => new { e.Status, e.AttemptedAt });
        });
    }
}
