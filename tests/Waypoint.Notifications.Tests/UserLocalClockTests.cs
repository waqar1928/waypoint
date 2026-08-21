using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class UserLocalClockTests
{
    [Fact]
    public void Computes_local_date_correctly_across_a_date_boundary_the_UTC_date_does_not_cross()
    {
        // 11pm Karachi time (UTC+5) on Aug 20 is 6pm UTC on Aug 20 - not a boundary case. Pick a
        // moment where UTC and local disagree on the calendar date instead: 2am Karachi time is
        // 9pm UTC the PREVIOUS day.
        var utcNow = new DateTimeOffset(2026, 8, 20, 21, 0, 0, TimeSpan.Zero); // Aug 20, 21:00 UTC
        var karachi = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi"); // UTC+5

        UserLocalClock.LocalDate(utcNow, karachi).Should().Be(new DateOnly(2026, 8, 21));
        UserLocalClock.LocalTimeOfDay(utcNow, karachi).Should().Be(new TimeOnly(2, 0));
    }

    [Fact]
    public void Computes_local_time_correctly_for_UTC_itself()
    {
        var utcNow = new DateTimeOffset(2026, 8, 20, 14, 30, 0, TimeSpan.Zero);

        UserLocalClock.LocalDate(utcNow, TimeZoneInfo.Utc).Should().Be(new DateOnly(2026, 8, 20));
        UserLocalClock.LocalTimeOfDay(utcNow, TimeZoneInfo.Utc).Should().Be(new TimeOnly(14, 30));
    }

    [Fact]
    public void Reflects_the_DST_adjusted_local_time_for_a_DST_observing_zone()
    {
        var newYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        // 12:00 UTC in July is 08:00 EDT (UTC-4); the same 12:00 UTC in January would be 07:00 EST
        // (UTC-5) - this proves the conversion picks up the seasonal offset automatically.
        var julyUtc = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero);
        var januaryUtc = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

        UserLocalClock.LocalTimeOfDay(julyUtc, newYork).Should().Be(new TimeOnly(8, 0));
        UserLocalClock.LocalTimeOfDay(januaryUtc, newYork).Should().Be(new TimeOnly(7, 0));
    }
}
