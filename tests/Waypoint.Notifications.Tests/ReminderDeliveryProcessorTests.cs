using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Notifications.Application.Push;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Tests;

/// <summary>
/// The production-critical decision logic extracted from ScheduledNotificationWorker (a
/// BackgroundService, which this codebase had no prior precedent for and can't be meaningfully
/// unit tested directly) - rate limiting, the next-best-action privacy gate, multi-device fan-out,
/// and the retry/deactivation lifecycle. Everything here was previously only covered by live
/// manual verification and reasoning in code comments; these tests make it deterministic and
/// automated.
/// </summary>
public class ReminderDeliveryProcessorTests
{
    private readonly IDeliveryHistoryRepository _deliveryHistory = Substitute.For<IDeliveryHistoryRepository>();
    private readonly IPushSubscriptionRepository _subscriptions = Substitute.For<IPushSubscriptionRepository>();
    private readonly INextBestActionSummaryProvider _nextBestAction = Substitute.For<INextBestActionSummaryProvider>();
    private readonly IPushSender _pushSender = Substitute.For<IPushSender>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 8, 21, 12, 0, 0, TimeSpan.Zero));
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _deliveryId = Guid.NewGuid();

    private ReminderDeliveryProcessor CreateProcessor() =>
        new(_deliveryHistory, _subscriptions, _nextBestAction, _pushSender, _timeProvider);

    private PushReminderCandidate MakeCandidate(bool detailedContent = false) =>
        new(_userId, "UTC", null, null, detailedContent);

    private NotificationDeliveryHistory MakeRow(int retryCount = 0) =>
        new() { Id = _deliveryId, UserId = _userId, ReminderKey = "daily-next-move:2026-08-21", RetryCount = retryCount };

    private static PushSubscription MakeSubscription(int consecutiveFailures = 0) =>
        new()
        {
            UserId = Guid.NewGuid(),
            Endpoint = $"https://fcm.googleapis.com/fcm/send/{Guid.NewGuid():N}",
            P256dhKey = "key",
            AuthKey = "auth",
            ConsecutiveFailureCount = consecutiveFailures,
        };

    private void ArrangeRow(NotificationDeliveryHistory row) =>
        _deliveryHistory.GetByIdAsync(_deliveryId, Arg.Any<CancellationToken>()).Returns(row);

    [Fact]
    public async Task Skips_when_the_daily_rate_limit_is_already_reached_and_never_looks_up_a_next_move()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(3);

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.Outcome.Should().Be(DeliveryOutcome.Skipped);
        result.SkipOrFailureReason.Should().Be("DailyRateLimitReached");
        await _deliveryHistory.Received(1).MarkSkippedAsync(_deliveryId, "DailyRateLimitReached", Arg.Any<CancellationToken>());
        await _nextBestAction.DidNotReceive().GetForUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _pushSender.DidNotReceive().SendAsync(Arg.Any<PushSubscription>(), Arg.Any<PushPayload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Skips_when_there_is_no_next_best_action_rather_than_sending_a_false_reminder()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((NextBestActionSummary?)null);

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.Outcome.Should().Be(DeliveryOutcome.Skipped);
        result.SkipOrFailureReason.Should().Be("NoNextMove");
        await _pushSender.DidNotReceive().SendAsync(Arg.Any<PushSubscription>(), Arg.Any<PushPayload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Skips_when_the_user_has_no_active_subscriptions()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Talk to five shop owners", "This is next because it's high priority."));
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([]);

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.Outcome.Should().Be(DeliveryOutcome.Skipped);
        result.SkipOrFailureReason.Should().Be("NoActiveSubscriptions");
    }

    [Fact]
    public async Task Sends_the_generic_payload_by_default_never_the_actual_action_title()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Talk to five shop owners about invoicing pain", "rationale"));
        var subscription = MakeSubscription();
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);

        await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(detailedContent: false), maxPerUserPerDay: 3, CancellationToken.None);

        await _pushSender.Received(1).SendAsync(
            subscription,
            Arg.Is<PushPayload>(p => p.Body == "Your next move is ready." && !p.Body.Contains("invoicing")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sends_the_actual_action_title_only_when_detailed_content_is_explicitly_enabled()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Talk to five shop owners about invoicing pain", "rationale"));
        var subscription = MakeSubscription();
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);

        await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(detailedContent: true), maxPerUserPerDay: 3, CancellationToken.None);

        await _pushSender.Received(1).SendAsync(
            subscription,
            Arg.Is<PushPayload>(p => p.Body == "Talk to five shop owners about invoicing pain"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Multiple_devices_one_succeeding_is_enough_to_mark_the_reminder_sent()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var goodSubscription = MakeSubscription();
        var badSubscription = MakeSubscription();
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([goodSubscription, badSubscription]);
        _pushSender.SendAsync(goodSubscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _pushSender.SendAsync(badSubscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PushDeliveryException(isPermanent: false, statusCode: 503, "temporary failure")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.Outcome.Should().Be(DeliveryOutcome.Sent);
        result.SubscriptionAttempts.Should().HaveCount(2);
        result.SubscriptionAttempts.Single(a => a.SubscriptionId == goodSubscription.Id).Succeeded.Should().BeTrue();
        result.SubscriptionAttempts.Single(a => a.SubscriptionId == badSubscription.Id).Succeeded.Should().BeFalse();
        await _subscriptions.Received(1).RecordSuccessAsync(goodSubscription, Arg.Any<CancellationToken>());
        await _subscriptions.Received(1).RecordFailureAsync(badSubscription, Arg.Any<CancellationToken>());
        await _deliveryHistory.Received(1).MarkSentAsync(_deliveryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_permanent_404_or_410_deactivates_the_subscription_immediately()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var subscription = MakeSubscription(consecutiveFailures: 0);
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);
        _pushSender.SendAsync(subscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PushDeliveryException(isPermanent: true, statusCode: 410, "gone")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.SubscriptionAttempts.Single().Deactivated.Should().BeTrue();
        await _subscriptions.Received(1).DeactivateAsync(subscription, "Gone410", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_transient_failure_below_the_deactivation_threshold_does_not_deactivate()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var subscription = MakeSubscription(consecutiveFailures: 1); // this failure would make it 2, below the threshold of 5
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);
        _pushSender.SendAsync(subscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PushDeliveryException(isPermanent: false, statusCode: 503, "temporary")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.SubscriptionAttempts.Single().Deactivated.Should().BeFalse();
        await _subscriptions.DidNotReceive().DeactivateAsync(Arg.Any<PushSubscription>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_transient_failure_reaching_the_threshold_deactivates_even_though_no_single_failure_was_permanent()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var subscription = MakeSubscription(consecutiveFailures: 4); // this failure makes it 5 - the threshold
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);
        _pushSender.SendAsync(subscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PushDeliveryException(isPermanent: false, statusCode: 503, "temporary")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.SubscriptionAttempts.Single().Deactivated.Should().BeTrue();
        await _subscriptions.Received(1).DeactivateAsync(subscription, "TooManyFailures", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unexpected_non_PushDeliveryException_is_treated_as_a_transient_failure()
    {
        ArrangeRow(MakeRow());
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var subscription = MakeSubscription(consecutiveFailures: 4); // one more unexpected failure reaches the threshold too
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);
        _pushSender.SendAsync(subscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("malformed key")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.SubscriptionAttempts.Single().Deactivated.Should().BeTrue();
        await _subscriptions.Received(1).DeactivateAsync(subscription, "TooManyFailures", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task All_devices_failing_with_retries_remaining_leaves_the_reminder_to_be_retried_later()
    {
        ArrangeRow(MakeRow(retryCount: 0)); // MaxRetries is 3, so retryCount 0 -> next attempt would be #2, retries remain
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var subscription = MakeSubscription();
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);
        _pushSender.SendAsync(subscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PushDeliveryException(isPermanent: false, statusCode: 503, "temporary")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.Outcome.Should().Be(DeliveryOutcome.RetryLater);
        await _deliveryHistory.Received(1).MarkAttemptedAgainAsync(_deliveryId, Arg.Any<CancellationToken>());
        await _deliveryHistory.DidNotReceive().MarkFailedAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task All_devices_failing_after_the_max_retry_count_marks_the_reminder_permanently_failed()
    {
        ArrangeRow(MakeRow(retryCount: 2)); // MaxRetries is 3 - this is the 3rd and final attempt
        _deliveryHistory.CountSentInWindowAsync(_userId, Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>()).Returns(0);
        _nextBestAction.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new NextBestActionSummary(Guid.NewGuid(), "Title", "rationale"));
        var subscription = MakeSubscription();
        _subscriptions.GetActiveForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns([subscription]);
        _pushSender.SendAsync(subscription, Arg.Any<PushPayload>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new PushDeliveryException(isPermanent: false, statusCode: 503, "temporary")));

        var result = await CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        result.Outcome.Should().Be(DeliveryOutcome.Failed);
        result.SkipOrFailureReason.Should().Be("MaxRetriesExceeded");
        await _deliveryHistory.Received(1).MarkFailedAsync(_deliveryId, "MaxRetriesExceeded", Arg.Any<CancellationToken>());
        await _deliveryHistory.DidNotReceive().MarkAttemptedAgainAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_clearly_if_asked_to_process_a_delivery_row_that_does_not_exist()
    {
        _deliveryHistory.GetByIdAsync(_deliveryId, Arg.Any<CancellationToken>()).Returns((NotificationDeliveryHistory?)null);

        var act = () => CreateProcessor().ProcessAsync(_deliveryId, MakeCandidate(), maxPerUserPerDay: 3, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

/// <summary>Minimal hand-written TimeProvider test double - same pattern already used by
/// GetNextBestActionQueryHandlerTests (Waypoint.Actions.Tests), kept local here rather than
/// pulling in Microsoft.Extensions.TimeProvider.Testing just for one fixed-clock fixture.</summary>
internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
