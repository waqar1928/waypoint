using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application.Push;

/// <summary>Persistence port for NotificationDeliveryHistory rows, scoped to exactly what
/// ReminderDeliveryProcessor needs once a row has already been claimed (see
/// ScheduledNotificationWorker's TryClaimAsync/stale-sweep raw SQL, which stays in Infrastructure
/// since it's Postgres-specific - claiming and processing are different concerns). Extracted as an
/// interface specifically so ReminderDeliveryProcessor's business logic (rate limit, no-next-move,
/// retry-vs-fail-after-max-retries) can be unit tested with a mock, instead of only being provable
/// by running against a real database.</summary>
public interface IDeliveryHistoryRepository
{
    Task<NotificationDeliveryHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Count of this user's Sent deliveries within [localDayStartUtc, localDayEndUtc) -
    /// the caller computes those bounds from the user's own local calendar day (see
    /// UserLocalClock), never a server/UTC day boundary.</summary>
    Task<int> CountSentInWindowAsync(Guid userId, DateTimeOffset localDayStartUtc, DateTimeOffset localDayEndUtc, CancellationToken cancellationToken);

    Task MarkSkippedAsync(Guid id, string reason, CancellationToken cancellationToken);
    Task MarkSentAsync(Guid id, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid id, string reason, CancellationToken cancellationToken);

    /// <summary>Leaves Status = Attempted but refreshes AttemptedAt - used when every subscription
    /// send failed but retries remain. The stale-attempt sweep (Infrastructure) is what actually
    /// increments RetryCount when it re-claims this row later; this method deliberately does not,
    /// to keep "was this claimed again" and "how many times has processing been attempted"
    /// tracked in exactly one place each.</summary>
    Task MarkAttemptedAgainAsync(Guid id, CancellationToken cancellationToken);
}
