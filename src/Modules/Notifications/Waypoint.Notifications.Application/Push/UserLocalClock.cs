namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// Converts "now" (always UTC, from TimeProvider) into a user's local calendar date / time-of-day.
/// Always computed fresh from the current UTC instant and the user's current TimeZoneInfo - never
/// pre-stored - so DST transitions are handled automatically by .NET's own tzdata (see
/// SafeTimeZoneResolver) and a timezone change takes effect on the very next worker tick with no
/// special-casing anywhere else in this module.
/// </summary>
public static class UserLocalClock
{
    public static DateOnly LocalDate(DateTimeOffset utcNow, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utcNow, zone).DateTime);

    public static TimeOnly LocalTimeOfDay(DateTimeOffset utcNow, TimeZoneInfo zone) =>
        TimeOnly.FromDateTime(TimeZoneInfo.ConvertTime(utcNow, zone).DateTime);
}
