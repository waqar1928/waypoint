using Waypoint.Common;

namespace Waypoint.Notifications.Application.Push;

public enum DeliveryOutcome { Sent, Skipped, RetryLater, Failed }

public sealed record SubscriptionAttemptOutcome(
    Guid SubscriptionId, bool Succeeded, bool Deactivated, string? DeactivationReason, string? FailureDetail);

/// <summary>Everything that happened processing one claimed reminder - returned rather than
/// logged directly, so this whole class stays free of any logging dependency and is asserted on
/// by value in tests instead of by mocking a logger. The caller (ScheduledNotificationWorker,
/// Infrastructure) does its own logging from this.</summary>
public sealed record DeliveryResult(
    DeliveryOutcome Outcome,
    string? SkipOrFailureReason,
    IReadOnlyList<SubscriptionAttemptOutcome> SubscriptionAttempts);

/// <summary>
/// Everything that happens once a logical reminder has already been claimed (see
/// NotificationDeliveryHistory's UNIQUE(UserId, ReminderKey) and ScheduledNotificationWorker's
/// claim/sweep SQL, which stay in Infrastructure - claiming is Postgres-specific, processing is
/// plain business logic): daily rate limit, resolve the next best action (the single source of
/// truth - see INextBestActionSummaryProvider), build the privacy-appropriate payload, fan out to
/// every active subscription, and decide the outcome. Extracted from the BackgroundService
/// specifically so this - the actual production-critical decision logic - is unit testable without
/// a real database, a real BackgroundService host, or a real push service.
/// </summary>
public sealed class ReminderDeliveryProcessor(
    IDeliveryHistoryRepository deliveryHistoryRepository,
    IPushSubscriptionRepository subscriptionRepository,
    INextBestActionSummaryProvider nextBestActionProvider,
    IPushSender pushSender,
    TimeProvider timeProvider)
{
    public const int MaxRetries = 3;

    public async Task<DeliveryResult> ProcessAsync(
        Guid deliveryId, PushReminderCandidate candidate, int maxPerUserPerDay, CancellationToken cancellationToken)
    {
        var row = await deliveryHistoryRepository.GetByIdAsync(deliveryId, cancellationToken)
            ?? throw new InvalidOperationException($"Delivery history row {deliveryId} was not found - it must be claimed before processing.");

        var (zone, _) = SafeTimeZoneResolver.Resolve(candidate.TimeZone);
        var localToday = UserLocalClock.LocalDate(timeProvider.GetUtcNow(), zone);
        var dayStartLocal = localToday.ToDateTime(TimeOnly.MinValue);
        var dayStart = new DateTimeOffset(dayStartLocal, zone.GetUtcOffset(dayStartLocal));
        var dayEnd = dayStart.AddDays(1);

        var sentToday = await deliveryHistoryRepository.CountSentInWindowAsync(candidate.UserId, dayStart, dayEnd, cancellationToken);
        if (!DailyRateLimit.IsUnderLimit(sentToday, maxPerUserPerDay))
        {
            await deliveryHistoryRepository.MarkSkippedAsync(row.Id, "DailyRateLimitReached", cancellationToken);
            return new DeliveryResult(DeliveryOutcome.Skipped, "DailyRateLimitReached", []);
        }

        // The single source of truth for "what's next" - never a competing recommendation
        // computed here. Also doubles as the "is there actually anything to notify about" check:
        // sending "your next move is ready" when there isn't one would be false and confusing, so
        // no next move means no notification, not a fallback generic send.
        var nextMove = await nextBestActionProvider.GetForUserAsync(candidate.UserId, cancellationToken);
        if (nextMove is null)
        {
            await deliveryHistoryRepository.MarkSkippedAsync(row.Id, "NoNextMove", cancellationToken);
            return new DeliveryResult(DeliveryOutcome.Skipped, "NoNextMove", []);
        }

        var subscriptions = await subscriptionRepository.GetActiveForUserAsync(candidate.UserId, cancellationToken);
        if (subscriptions.Count == 0)
        {
            await deliveryHistoryRepository.MarkSkippedAsync(row.Id, "NoActiveSubscriptions", cancellationToken);
            return new DeliveryResult(DeliveryOutcome.Skipped, "NoActiveSubscriptions", []);
        }

        // The one place content privacy is decided - never in the service worker (see
        // apps/web/public/sw.js, which has no content-decision logic at all) and never anywhere
        // else in this class either. PushPayloadBuilder.BuildDailyNextMove is the sole call site.
        var payload = PushPayloadBuilder.BuildDailyNextMove(candidate.DetailedContentEnabled, nextMove.Title);

        var attempts = new List<SubscriptionAttemptOutcome>();
        foreach (var subscription in subscriptions)
        {
            attempts.Add(await SendToOneSubscriptionAsync(subscription, payload, cancellationToken));
        }

        var anySucceeded = attempts.Any(a => a.Succeeded);
        if (anySucceeded)
        {
            await deliveryHistoryRepository.MarkSentAsync(row.Id, cancellationToken);
            return new DeliveryResult(DeliveryOutcome.Sent, null, attempts);
        }

        if (row.RetryCount + 1 >= MaxRetries)
        {
            await deliveryHistoryRepository.MarkFailedAsync(row.Id, "MaxRetriesExceeded", cancellationToken);
            return new DeliveryResult(DeliveryOutcome.Failed, "MaxRetriesExceeded", attempts);
        }

        // Left as Attempted - the stale-attempt sweep (Infrastructure) picks this up again on a
        // later tick, incrementing RetryCount itself when it does. Same row throughout; never a
        // new one - that's what keeps this idempotent even across retries.
        await deliveryHistoryRepository.MarkAttemptedAgainAsync(row.Id, cancellationToken);
        return new DeliveryResult(DeliveryOutcome.RetryLater, null, attempts);
    }

    private async Task<SubscriptionAttemptOutcome> SendToOneSubscriptionAsync(
        Waypoint.Notifications.Domain.PushSubscription subscription, PushPayload payload, CancellationToken cancellationToken)
    {
        try
        {
            await pushSender.SendAsync(subscription, payload, cancellationToken);
            await subscriptionRepository.RecordSuccessAsync(subscription, cancellationToken);
            return new SubscriptionAttemptOutcome(subscription.Id, Succeeded: true, Deactivated: false, DeactivationReason: null, FailureDetail: null);
        }
        catch (PushDeliveryException ex)
        {
            await subscriptionRepository.RecordFailureAsync(subscription, cancellationToken);
            var shouldDeactivate = PushSubscriptionLifecycle.ShouldDeactivateAfterFailure(
                subscription.ConsecutiveFailureCount + 1, ex.IsPermanent);

            if (shouldDeactivate)
            {
                var reason = ex.IsPermanent ? "Gone410" : "TooManyFailures";
                await subscriptionRepository.DeactivateAsync(subscription, reason, cancellationToken);
                return new SubscriptionAttemptOutcome(subscription.Id, Succeeded: false, Deactivated: true, reason, ex.Message);
            }

            return new SubscriptionAttemptOutcome(subscription.Id, Succeeded: false, Deactivated: false, DeactivationReason: null, ex.Message);
        }
        catch (Exception ex)
        {
            // Anything not shaped as a PushDeliveryException (a malformed subscription key
            // causing an encryption failure, a DNS/connect failure from PrivateNetworkGuard, etc.)
            // - treated the same as a transient, non-permanent failure for lifecycle purposes, so
            // a subscription that's persistently broken in some way this class didn't anticipate
            // still eventually deactivates instead of being retried forever.
            await subscriptionRepository.RecordFailureAsync(subscription, cancellationToken);
            var shouldDeactivate = PushSubscriptionLifecycle.ShouldDeactivateAfterFailure(
                subscription.ConsecutiveFailureCount + 1, isPermanentFailure: false);

            if (shouldDeactivate)
            {
                await subscriptionRepository.DeactivateAsync(subscription, "TooManyFailures", cancellationToken);
                return new SubscriptionAttemptOutcome(subscription.Id, Succeeded: false, Deactivated: true, "TooManyFailures", ex.Message);
            }

            return new SubscriptionAttemptOutcome(subscription.Id, Succeeded: false, Deactivated: false, DeactivationReason: null, ex.Message);
        }
    }
}
