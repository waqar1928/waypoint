using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Waypoint.Common;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Infrastructure;

/// <summary>
/// The whole P1 push-notification pipeline's engine. A plain PostgreSQL-polling BackgroundService,
/// not a distributed queue - per the approved architecture review, Drevia's actual scale (one API
/// container, low daily notification volume) doesn't need Redis/Hangfire/Quartz/RabbitMQ, and this
/// design already tolerates multiple overlapping instances safely (see below), so there's nothing
/// a heavier queue would buy that isn't already handled here.
///
/// Every tick does two things:
///  1. ClaimNewRemindersAsync - for every user opted into the daily reminder, works out whether
///     it's due (past their local reminder time, not in quiet hours), and atomically claims it via
///     `INSERT ... ON CONFLICT (user_id, reminder_key) DO NOTHING` - Postgres serializes concurrent
///     inserts on the same unique key natively, so two overlapping ticks/instances can never both
///     "win" the same logical reminder. This is the actual duplicate-prevention mechanism; see
///     NotificationDeliveryHistory's unique index.
///  2. SweepStaleAttemptsAsync - retries reminders left in an unfinished `Attempted` state (a
///     transient send failure, or the process restarting mid-delivery). This IS the case where
///     multiple workers could genuinely compete for the same existing rows, so it uses
///     `SELECT ... FOR UPDATE SKIP LOCKED` to claim a batch atomically - a second worker simply
///     never sees a row this one already locked.
///
/// The actual decision logic once a reminder is claimed (rate limit, next-best-action lookup,
/// payload privacy, per-subscription fan-out, retry/deactivation) lives in
/// Waypoint.Notifications.Application.Push.ReminderDeliveryProcessor, not here - this class only
/// owns claiming (Postgres-specific SQL) and logs whatever DeliveryResult that processor returns.
/// That split is what makes the actual business logic unit testable without a real database, a
/// real BackgroundService host, or a real push service.
///
/// Graceful shutdown: BackgroundService's own stoppingToken cancellation is respected throughout;
/// an in-flight tick finishes its current (small, bounded) batch rather than being torn down
/// mid-write, and Task.Delay between ticks exits immediately on cancellation.
/// </summary>
public sealed class ScheduledNotificationWorker(
    IServiceScopeFactory scopeFactory,
    VapidOptions vapidOptions,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<ScheduledNotificationWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan StaleClaimCutoff = TimeSpan.FromMinutes(5);
    private const int ClaimBatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = TimeSpan.FromSeconds(
            configuration.GetValue("Waypoint:Notifications:Push:PollingIntervalSeconds", 120));
        var reminderLocalTime = TimeOnly.TryParse(
            configuration["Waypoint:Notifications:Push:DailyReminderLocalTime"], out var parsedTime)
            ? parsedTime
            : new TimeOnly(9, 0);
        var maxPerUserPerDay = configuration.GetValue(
            "Waypoint:Notifications:Push:MaxPerUserPerDay", DailyRateLimit.DefaultMaxPerUserPerDay);

        logger.LogInformation(
            "ScheduledNotificationWorker starting. VapidConfigured={VapidConfigured} PollingInterval={PollingInterval} " +
            "ReminderLocalTime={ReminderLocalTime} MaxPerUserPerDay={MaxPerUserPerDay}",
            vapidOptions.IsConfigured, pollingInterval, reminderLocalTime, maxPerUserPerDay);

        if (!vapidOptions.IsConfigured)
        {
            logger.LogWarning(
                "VAPID keys are not configured - push notifications are disabled. Set " +
                "Waypoint:Notifications:Push:VapidPublicKey / VapidPrivateKey / VapidSubject to enable.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (vapidOptions.IsConfigured)
            {
                try
                {
                    await ClaimNewRemindersAsync(reminderLocalTime, maxPerUserPerDay, stoppingToken);
                    await SweepStaleAttemptsAsync(maxPerUserPerDay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Never let one bad tick kill the worker loop - it retries on the next tick.
                    logger.LogError(ex, "ScheduledNotificationWorker tick failed unexpectedly.");
                }
            }

            try
            {
                await Task.Delay(pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("ScheduledNotificationWorker stopping.");
    }

    private async Task ClaimNewRemindersAsync(TimeOnly reminderLocalTime, int maxPerUserPerDay, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var audienceProvider = scope.ServiceProvider.GetRequiredService<IPushReminderAudienceProvider>();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var processor = scope.ServiceProvider.GetRequiredService<ReminderDeliveryProcessor>();

        var candidates = await audienceProvider.GetDailyReminderCandidatesAsync(ct);
        var utcNow = timeProvider.GetUtcNow();

        foreach (var candidate in candidates)
        {
            ct.ThrowIfCancellationRequested();

            var (zone, usedFallback) = SafeTimeZoneResolver.Resolve(candidate.TimeZone);
            if (usedFallback)
            {
                logger.LogWarning(
                    "Invalid or missing timezone {TimeZone} for a push reminder candidate - falling back to UTC.",
                    candidate.TimeZone);
            }

            var localTime = UserLocalClock.LocalTimeOfDay(utcNow, zone);
            if (localTime < reminderLocalTime)
            {
                continue; // not due yet today
            }

            if (QuietHoursEvaluator.IsWithinQuietHours(localTime, candidate.QuietHoursStart, candidate.QuietHoursEnd))
            {
                continue; // delayed until quiet hours end - re-evaluated next tick, never skipped
            }

            var localDate = UserLocalClock.LocalDate(utcNow, zone);
            var reminderKey = ReminderKey.DailyNextMove(localDate);

            var claimedId = await TryClaimAsync(db, candidate.UserId, reminderKey, ct);
            if (claimedId is null)
            {
                continue; // already claimed/sent by this or another tick/instance - nothing to do
            }

            logger.LogInformation("Reminder claimed. ReminderKey={ReminderKey}", reminderKey);
            var result = await processor.ProcessAsync(claimedId.Value, candidate, maxPerUserPerDay, ct);
            LogResult(reminderKey, result);
        }
    }

    /// <summary>The actual duplicate-prevention mechanism. Postgres serializes concurrent inserts
    /// on the same unique key (user_id, reminder_key) natively - no explicit locking needed here,
    /// unlike the stale-attempt sweep below (which selects among EXISTING rows, a genuine
    /// multiple-workers-see-the-same-row race that SKIP LOCKED exists for).</summary>
    private static async Task<Guid?> TryClaimAsync(
        NotificationsDbContext db, Guid userId, string reminderKey, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var inserted = await db.Database.SqlQuery<Guid>(
            $"""
             INSERT INTO notifications_delivery_history (id, user_id, reminder_key, status, attempted_at, retry_count)
             VALUES ({id}, {userId}, {reminderKey}, 'Attempted', {now}, 0)
             ON CONFLICT (user_id, reminder_key) DO NOTHING
             RETURNING id
             """)
            .ToListAsync(ct);

        return inserted.Count == 0 ? null : id;
    }

    private async Task SweepStaleAttemptsAsync(int maxPerUserPerDay, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        var audienceProvider = scope.ServiceProvider.GetRequiredService<IPushReminderAudienceProvider>();
        var processor = scope.ServiceProvider.GetRequiredService<ReminderDeliveryProcessor>();

        var cutoff = DateTimeOffset.UtcNow - StaleClaimCutoff;

        // The genuine multi-consumer race in this design: unlike a fresh INSERT (serialized
        // natively by the unique constraint above), retrying EXISTING stale rows means multiple
        // overlapping worker ticks or app instances could otherwise all pick the same rows. This
        // atomically claims a batch and increments retry_count in one statement - SKIP LOCKED
        // means a second worker never even sees a row this one already locked.
        var claimedIds = await db.Database.SqlQuery<Guid>(
            $"""
             UPDATE notifications_delivery_history
             SET retry_count = retry_count + 1
             WHERE id IN (
                 SELECT id FROM notifications_delivery_history
                 WHERE status = 'Attempted' AND attempted_at < {cutoff} AND retry_count < {ReminderDeliveryProcessor.MaxRetries}
                 ORDER BY attempted_at
                 LIMIT {ClaimBatchSize}
                 FOR UPDATE SKIP LOCKED
             )
             RETURNING id
             """)
            .ToListAsync(ct);

        if (claimedIds.Count == 0)
        {
            return;
        }

        var rows = await db.DeliveryHistory.AsNoTracking().Where(d => claimedIds.Contains(d.Id)).ToListAsync(ct);
        var candidates = await audienceProvider.GetDailyReminderCandidatesAsync(ct);
        var candidateByUserId = candidates.ToDictionary(c => c.UserId);

        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();

            if (!candidateByUserId.TryGetValue(row.UserId, out var candidate))
            {
                // The user turned off push (or the daily reminder specifically) since this was
                // first claimed - it can never succeed now, so mark it done rather than retrying
                // it out to MaxRetries for no reason.
                await MarkSkippedAsync(db, row.Id, "AudienceNoLongerEligible", ct);
                continue;
            }

            logger.LogInformation("Retrying stale reminder attempt. ReminderKey={ReminderKey} RetryCount={RetryCount}",
                row.ReminderKey, row.RetryCount + 1);
            var result = await processor.ProcessAsync(row.Id, candidate, maxPerUserPerDay, ct);
            LogResult(row.ReminderKey, result);
        }
    }

    private static async Task MarkSkippedAsync(NotificationsDbContext db, Guid deliveryId, string reason, CancellationToken ct)
    {
        var row = await db.DeliveryHistory.FirstAsync(d => d.Id == deliveryId, ct);
        row.Status = Domain.DeliveryStatus.Skipped;
        row.FailureReason = reason;
        row.CompletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private void LogResult(string reminderKey, DeliveryResult result)
    {
        switch (result.Outcome)
        {
            case DeliveryOutcome.Sent:
                logger.LogInformation("Reminder sent. ReminderKey={ReminderKey} Subscriptions={Count}",
                    reminderKey, result.SubscriptionAttempts.Count(a => a.Succeeded));
                break;
            case DeliveryOutcome.Skipped:
                logger.LogInformation("Reminder skipped. ReminderKey={ReminderKey} Reason={Reason}",
                    reminderKey, result.SkipOrFailureReason);
                break;
            case DeliveryOutcome.Failed:
                logger.LogWarning("Reminder failed. ReminderKey={ReminderKey} Reason={Reason}",
                    reminderKey, result.SkipOrFailureReason);
                break;
            case DeliveryOutcome.RetryLater:
                logger.LogInformation("Reminder will be retried. ReminderKey={ReminderKey}", reminderKey);
                break;
        }

        foreach (var attempt in result.SubscriptionAttempts.Where(a => !a.Succeeded))
        {
            logger.LogWarning(
                "Push delivery failed. SubscriptionId={SubscriptionId} Deactivated={Deactivated} Reason={Reason}",
                attempt.SubscriptionId, attempt.Deactivated, attempt.DeactivationReason);
        }
    }
}
