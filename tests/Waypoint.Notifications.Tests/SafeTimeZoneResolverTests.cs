using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class SafeTimeZoneResolverTests
{
    [Fact]
    public void Resolves_UTC()
    {
        var (zone, usedFallback) = SafeTimeZoneResolver.Resolve("UTC");

        zone.Should().Be(TimeZoneInfo.Utc);
        usedFallback.Should().BeFalse();
    }

    [Fact]
    public void Resolves_Asia_Karachi_a_zone_with_no_DST()
    {
        var (zone, usedFallback) = SafeTimeZoneResolver.Resolve("Asia/Karachi");

        zone.Id.Should().Be("Asia/Karachi");
        zone.GetUtcOffset(new DateTime(2026, 1, 1)).Should().Be(TimeSpan.FromHours(5));
        zone.GetUtcOffset(new DateTime(2026, 7, 1)).Should().Be(TimeSpan.FromHours(5)); // no DST, no seasonal drift
        usedFallback.Should().BeFalse();
    }

    [Fact]
    public void Resolves_a_DST_observing_US_zone_with_the_correct_seasonal_offsets()
    {
        var (zone, usedFallback) = SafeTimeZoneResolver.Resolve("America/New_York");

        zone.Id.Should().Be("America/New_York");
        // EST in January, EDT in July - .NET's own tzdata handles this, no manual offset math.
        zone.GetUtcOffset(new DateTime(2026, 1, 1)).Should().Be(TimeSpan.FromHours(-5));
        zone.GetUtcOffset(new DateTime(2026, 7, 1)).Should().Be(TimeSpan.FromHours(-4));
        usedFallback.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Not/A/Real/Zone")]
    [InlineData("garbage")]
    public void Falls_back_to_UTC_and_reports_the_fallback_for_missing_or_invalid_values(string? invalidValue)
    {
        var (zone, usedFallback) = SafeTimeZoneResolver.Resolve(invalidValue);

        zone.Should().Be(TimeZoneInfo.Utc);
        usedFallback.Should().BeTrue();
    }
}
