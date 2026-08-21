namespace Waypoint.Notifications.Domain;

public enum DeliveryStatus { Attempted, Sent, Failed, Skipped }

/// <summary>
/// The idempotency ledger and audit trail for push reminders, in one table (see
/// docs/PRODUCTION_READINESS_AUDIT.md-style reasoning: same "why keep two tables when one append-
/// only fact table serves both" logic as AuditLogEntry). One row = one LOGICAL reminder for one
/// user - not one row per device. The unique index on (UserId, ReminderKey) is what actually
/// prevents duplicate sends: a worker tick, a second overlapping worker instance, or a retry after
/// a restart can all attempt to claim the same logical reminder, but only one INSERT ever succeeds
/// for a given (user, key) pair. See ScheduledNotificationWorker for how ReminderKey is built
/// (type + the user's own LOCAL calendar date, e.g. "daily-next-move:2026-08-21") and how the claim
/// itself works (INSERT ... ON CONFLICT (user_id, reminder_key) DO NOTHING).
/// </summary>
public sealed class NotificationDeliveryHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required string ReminderKey { get; init; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Attempted;
    public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Short machine code only ("Gone410", "NoNextMove", "NoActiveSubscriptions",
    /// "DailyRateLimitReached", "NetworkTimeout") - never notification body content, never a raw
    /// exception message.</summary>
    public string? FailureReason { get; set; }

    public int? HttpStatusCode { get; set; }
    public int RetryCount { get; set; }
}
