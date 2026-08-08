using FluentAssertions;
using Waypoint.Common;
using Waypoint.Goals.Application;
using Xunit;

namespace Waypoint.Goals.Tests;

public class HeuristicPlanDraftGeneratorTests
{
    private readonly HeuristicPlanDraftGenerator _generator = new();

    private static DreamSummary MakeDream(
        string? outcome = null, string? whoItHelps = null, string? problem = null, bool isBusinessShaped = false) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Cut waste for small manufacturers",
            "Help small manufacturers reduce waste.", "Purpose", whoItHelps, problem, outcome, "Motivation", "Impact",
            isBusinessShaped);

    [Fact]
    public void Generates_all_four_cascade_levels()
    {
        var result = _generator.Generate(MakeDream());

        result.FiveYearVision.Should().NotBeNullOrWhiteSpace();
        result.ThreeYearDirection.Should().NotBeNullOrWhiteSpace();
        result.OneYearGoal.Should().NotBeNullOrWhiteSpace();
        result.NinetyDayMission.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Never_produces_double_punctuation_when_source_text_ends_with_a_period()
    {
        var result = _generator.Generate(MakeDream(whoItHelps: "Small manufacturing shop owners."));

        result.ThreeYearDirection.Should().NotContain("..");
    }

    [Fact]
    public void Ninety_day_mission_frames_as_validation_when_business_shaped()
    {
        var result = _generator.Generate(MakeDream(isBusinessShaped: true));

        result.NinetyDayMission.Should().ContainEquivalentOf("validate");
    }
}
