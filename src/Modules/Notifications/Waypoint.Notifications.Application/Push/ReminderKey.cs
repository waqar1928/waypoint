namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// Builds the stable logical key that makes a reminder idempotent - see
/// NotificationDeliveryHistory's UNIQUE(UserId, ReminderKey) constraint, the actual mechanism that
/// prevents duplicate sends. The date must be the user's own LOCAL calendar date (see
/// UserLocalClock), not server/UTC date - otherwise a user near a timezone boundary could see
/// "today" flip at the wrong wall-clock moment for them, and two different local days could
/// collide onto the same UTC date or vice versa.
/// </summary>
public static class ReminderKey
{
    public const string DailyNextMoveType = "daily-next-move";

    public static string DailyNextMove(DateOnly userLocalDate) =>
        $"{DailyNextMoveType}:{userLocalDate:yyyy-MM-dd}";
}
