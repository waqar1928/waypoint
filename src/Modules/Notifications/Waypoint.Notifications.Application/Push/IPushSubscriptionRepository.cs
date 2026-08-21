using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application.Push;

public interface IPushSubscriptionRepository
{
    /// <summary>Insert-or-reactivate keyed on Endpoint (globally unique - see
    /// notifications_push_subscriptions' unique index). The same browser subscribing again
    /// (permission re-granted, or a different Drevia account signing into the same browser)
    /// reuses this same row rather than creating a duplicate.</summary>
    Task<PushSubscription> UpsertAsync(
        Guid userId, string endpoint, string p256dhKey, string authKey, string? userAgent,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PushSubscription>> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Every subscription regardless of status - backs a future "manage your devices"
    /// list and the ownership check in UnsubscribeFromPushCommand.</summary>
    Task<IReadOnlyList<PushSubscription>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<PushSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task DeactivateAsync(PushSubscription subscription, string reason, CancellationToken cancellationToken);

    Task RecordSuccessAsync(PushSubscription subscription, CancellationToken cancellationToken);

    /// <summary>Increments ConsecutiveFailureCount and sets LastFailureAt; does NOT decide
    /// deactivation itself (see PushSubscriptionLifecycle.ShouldDeactivateAfterFailure - a pure,
    /// independently testable decision the caller makes and then calls DeactivateAsync for, if
    /// warranted).</summary>
    Task RecordFailureAsync(PushSubscription subscription, CancellationToken cancellationToken);
}
