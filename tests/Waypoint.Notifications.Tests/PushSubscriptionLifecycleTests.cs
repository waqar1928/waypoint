using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class PushSubscriptionLifecycleTests
{
    [Fact]
    public void A_permanent_failure_deactivates_immediately_regardless_of_failure_count()
    {
        PushSubscriptionLifecycle.ShouldDeactivateAfterFailure(consecutiveFailureCountAfterThisFailure: 1, isPermanentFailure: true)
            .Should().BeTrue();
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(6, true)]
    public void A_transient_failure_only_deactivates_once_it_reaches_the_threshold(int consecutiveFailures, bool expectedDeactivate)
    {
        PushSubscriptionLifecycle.ShouldDeactivateAfterFailure(consecutiveFailures, isPermanentFailure: false)
            .Should().Be(expectedDeactivate);
    }
}
