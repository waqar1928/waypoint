namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// Decides whether a subscription should be deactivated after a failed delivery. A permanent
/// failure (404/410 Gone - the push service's explicit "this will never work again" signal)
/// deactivates immediately, regardless of how many prior failures there were. A transient failure
/// (timeout/5xx) only deactivates once it's happened MaxConsecutiveFailures times across separate
/// attempts - not in one burst - so a single bad network blip never takes a working device offline.
/// A success anywhere in between resets the counter back to zero (see
/// PushSubscriptionRepository.RecordSuccessAsync), so this threshold is about a sustained pattern
/// of failure, not a raw lifetime total.
/// </summary>
public static class PushSubscriptionLifecycle
{
    public const int MaxConsecutiveFailures = 5;

    public static bool ShouldDeactivateAfterFailure(int consecutiveFailureCountAfterThisFailure, bool isPermanentFailure) =>
        isPermanentFailure || consecutiveFailureCountAfterThisFailure >= MaxConsecutiveFailures;
}
