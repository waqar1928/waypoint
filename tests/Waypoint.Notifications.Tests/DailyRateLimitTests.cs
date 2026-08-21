using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class DailyRateLimitTests
{
    [Theory]
    [InlineData(0, 3, true)]
    [InlineData(2, 3, true)]
    [InlineData(3, 3, false)]
    [InlineData(4, 3, false)]
    public void Enforces_the_configured_cap(int sentToday, int cap, bool expectedUnderLimit)
    {
        DailyRateLimit.IsUnderLimit(sentToday, cap).Should().Be(expectedUnderLimit);
    }

    [Fact]
    public void Default_cap_is_conservative_since_P1_ships_only_one_reminder_type()
    {
        DailyRateLimit.DefaultMaxPerUserPerDay.Should().Be(3);
    }
}
