namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// Quiet hours delay a reminder until the window ends - they never skip it and never collapse it
/// into anything else (see ScheduledNotificationWorker: a reminder inside quiet hours simply isn't
/// claimed this tick, and is re-evaluated on the next one until it's eligible - the
/// UNIQUE(UserId, ReminderKey) constraint means it's still only ever sent once, whenever that turns
/// out to be).
///
/// Both QuietHoursStart and QuietHoursEnd being null means "not configured" - always eligible.
/// A window where Start &gt; End (e.g. 22:00 -> 07:00) wraps midnight, which is the expected common
/// case; a window where Start &lt;= End is an unusual but valid same-day window (e.g. 13:00 -> 14:00).
///
/// Worked example for 22:00 -> 07:00:
///   23:00 -> quiet (delayed to 07:00)
///   03:00 -> quiet (delayed to 07:00)
///   06:59 -> quiet (delayed to 07:00)
///   07:00 -> NOT quiet (the end boundary is exclusive - this is the first eligible moment)
///   07:01 -> NOT quiet
/// </summary>
public static class QuietHoursEvaluator
{
    public static bool IsWithinQuietHours(TimeOnly localNow, TimeOnly? start, TimeOnly? end)
    {
        if (start is null || end is null)
        {
            return false;
        }

        return start.Value <= end.Value
            ? localNow >= start.Value && localNow < end.Value
            : localNow >= start.Value || localNow < end.Value;
    }
}
