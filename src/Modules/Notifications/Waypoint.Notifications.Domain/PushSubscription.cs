namespace Waypoint.Notifications.Domain;

public enum PushSubscriptionStatus { Active, Deactivated }

/// <summary>
/// One browser/device's Web Push registration for one user. A user can have many of these
/// (multiple devices) - see notifications_delivery_history for how a single logical daily
/// reminder still only counts once against that user's rate limit no matter how many active
/// subscriptions it fans out to. Deliberately not a Waypoint.Common.Entity subclass: like
/// Notification, this is closer to a device-registration record than a domain aggregate, and
/// there's no per-request "current user" concept for the background worker that writes to most of
/// these fields (LastSeenAt/LastSuccessAt/etc.), so the Entity base's CreatedBy/UpdatedBy audit
/// fields wouldn't mean anything here.
/// </summary>
public sealed class PushSubscription
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }

    /// <summary>The browser-provided push endpoint URL - globally unique per browser+origin
    /// registration, and the natural upsert key for "is this the same device re-subscribing."
    /// Never logged in full (see ScheduledNotificationWorker's logging) - while not a secret on
    /// its own, it's still sensitive: whoever holds it (together with the auth/p256dh keys, which
    /// ARE treated as secrets) could send arbitrary pushes to this device.</summary>
    public required string Endpoint { get; init; }

    public required string P256dhKey { get; init; }
    public required string AuthKey { get; init; }

    /// <summary>Informational only - lets a future "manage your devices" UI show something like
    /// "Chrome on Mac" instead of an opaque endpoint. Never used for any security decision.</summary>
    public string? UserAgent { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSuccessAt { get; set; }
    public DateTimeOffset? LastFailureAt { get; set; }
    public int ConsecutiveFailureCount { get; set; }
    public PushSubscriptionStatus Status { get; set; } = PushSubscriptionStatus.Active;
    public DateTimeOffset? DeactivatedAt { get; set; }

    /// <summary>Short machine code only ("Gone410", "UserUnsubscribed", "TooManyFailures") - never
    /// free text, and never anything derived from notification content.</summary>
    public string? DeactivatedReason { get; set; }

    public static PushSubscription Create(Guid userId, string endpoint, string p256dhKey, string authKey, string? userAgent) =>
        new()
        {
            UserId = userId,
            Endpoint = endpoint,
            P256dhKey = p256dhKey,
            AuthKey = authKey,
            UserAgent = userAgent,
        };
}
