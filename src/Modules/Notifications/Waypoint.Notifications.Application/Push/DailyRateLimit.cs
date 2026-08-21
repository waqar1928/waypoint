namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// A hard backstop against notification spam, not a target - P1 ships exactly one reminder type
/// (daily next-move), so this is deliberately conservative. Counts LOGICAL notifications
/// (one NotificationDeliveryHistory row with Status = Sent), never per-device: a user with three
/// active subscriptions who receives their one daily reminder still only spends "1" here, since
/// it's the same logical notification fanned out to every active device in one claim. Retries
/// never consume additional quota either - a retry updates the existing Attempted/Failed row for
/// the same ReminderKey, it doesn't create a new logical notification. The "today" used to count
/// must be the user's own local calendar date (see UserLocalClock), not server/UTC time.
/// </summary>
public static class DailyRateLimit
{
    public const int DefaultMaxPerUserPerDay = 3;

    public static bool IsUnderLimit(int sentCountToday, int maxPerUserPerDay) =>
        sentCountToday < maxPerUserPerDay;
}
