namespace Waypoint.Notifications.Application.Push;

/// <summary>
/// Resolves a stored Profile.TimeZone string to a real TimeZoneInfo, safely. An invalid or missing
/// value must never crash the worker - it falls back to UTC instead, and callers are told a
/// fallback happened so they can log a warning (this class stays a pure function with no logger
/// dependency of its own, which keeps it trivially unit-testable).
/// </summary>
public static class SafeTimeZoneResolver
{
    public static (TimeZoneInfo Zone, bool UsedFallback) Resolve(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return (TimeZoneInfo.FindSystemTimeZoneById(timeZoneId), false);
            }
            catch (TimeZoneNotFoundException)
            {
                // fall through to UTC
            }
            catch (InvalidTimeZoneException)
            {
                // fall through to UTC
            }
        }

        return (TimeZoneInfo.Utc, true);
    }
}
