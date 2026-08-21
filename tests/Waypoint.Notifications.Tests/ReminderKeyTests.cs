using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class ReminderKeyTests
{
    [Fact]
    public void Builds_the_expected_key_shape()
    {
        ReminderKey.DailyNextMove(new DateOnly(2026, 8, 21)).Should().Be("daily-next-move:2026-08-21");
    }

    [Fact]
    public void Different_local_dates_produce_different_keys()
    {
        var today = ReminderKey.DailyNextMove(new DateOnly(2026, 8, 21));
        var tomorrow = ReminderKey.DailyNextMove(new DateOnly(2026, 8, 22));

        today.Should().NotBe(tomorrow);
    }

    [Fact]
    public void The_same_local_date_always_produces_the_same_key()
    {
        var first = ReminderKey.DailyNextMove(new DateOnly(2026, 8, 21));
        var second = ReminderKey.DailyNextMove(new DateOnly(2026, 8, 21));

        first.Should().Be(second);
    }
}
