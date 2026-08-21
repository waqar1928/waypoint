using FluentAssertions;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Tests;

public class PushPayloadBuilderTests
{
    [Fact]
    public void Detailed_content_disabled_always_produces_the_generic_default_body()
    {
        var payload = PushPayloadBuilder.BuildDailyNextMove(
            detailedContentEnabled: false, nextMoveTitle: "Talk to five shop owners about invoicing pain");

        payload.Body.Should().Be("Your next move is ready.");
        payload.Title.Should().Be("Drevia");
        // The default payload must never leak the actual action title, even though one was
        // available - this is the actual privacy guarantee, not just a default when nothing
        // exists to show.
        payload.Body.Should().NotContain("invoicing");
    }

    [Fact]
    public void Detailed_content_enabled_shows_the_actual_next_move_title()
    {
        var payload = PushPayloadBuilder.BuildDailyNextMove(
            detailedContentEnabled: true, nextMoveTitle: "Talk to five shop owners about invoicing pain");

        payload.Body.Should().Be("Talk to five shop owners about invoicing pain");
    }

    [Fact]
    public void Detailed_content_enabled_but_no_title_available_falls_back_to_generic_body()
    {
        var payload = PushPayloadBuilder.BuildDailyNextMove(detailedContentEnabled: true, nextMoveTitle: null);

        payload.Body.Should().Be("Your next move is ready.");
    }

    [Fact]
    public void Never_exposes_a_URL_outside_the_app()
    {
        var payload = PushPayloadBuilder.BuildDailyNextMove(detailedContentEnabled: true, nextMoveTitle: "Anything");

        payload.Url.Should().StartWith("/app/");
    }
}
