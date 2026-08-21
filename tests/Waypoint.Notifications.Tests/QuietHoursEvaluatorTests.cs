using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class QuietHoursEvaluatorTests
{
    // The exact worked example from the approved P1 design: quiet hours 22:00 -> 07:00.
    [Theory]
    [InlineData(21, 59, false)] // just before the start boundary - still eligible
    [InlineData(22, 0, true)] // start boundary is inclusive - the first quiet moment
    [InlineData(23, 0, true)]
    [InlineData(3, 0, true)]
    [InlineData(6, 59, true)]
    [InlineData(7, 0, false)] // end boundary is exclusive - first eligible moment
    [InlineData(7, 1, false)]
    public void Evaluates_a_midnight_wrapping_window_correctly(int hour, int minute, bool expectedQuiet)
    {
        var start = new TimeOnly(22, 0);
        var end = new TimeOnly(7, 0);

        QuietHoursEvaluator.IsWithinQuietHours(new TimeOnly(hour, minute), start, end).Should().Be(expectedQuiet);
    }

    [Theory]
    [InlineData(12, 59, false)]
    [InlineData(13, 0, true)]
    [InlineData(13, 30, true)]
    [InlineData(13, 59, true)]
    [InlineData(14, 0, false)]
    public void Evaluates_a_same_day_non_wrapping_window_correctly(int hour, int minute, bool expectedQuiet)
    {
        var start = new TimeOnly(13, 0);
        var end = new TimeOnly(14, 0);

        QuietHoursEvaluator.IsWithinQuietHours(new TimeOnly(hour, minute), start, end).Should().Be(expectedQuiet);
    }

    [Fact]
    public void Always_eligible_when_quiet_hours_are_not_configured()
    {
        QuietHoursEvaluator.IsWithinQuietHours(new TimeOnly(3, 0), null, null).Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "07:00")]
    [InlineData("22:00", null)]
    public void Always_eligible_when_only_one_side_is_configured(string? startText, string? endText)
    {
        var start = startText is null ? (TimeOnly?)null : TimeOnly.Parse(startText);
        var end = endText is null ? (TimeOnly?)null : TimeOnly.Parse(endText);

        QuietHoursEvaluator.IsWithinQuietHours(new TimeOnly(23, 0), start, end).Should().BeFalse();
    }
}
