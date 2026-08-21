using FluentAssertions;
using Waypoint.Common;
using Waypoint.Goals.Application;
using Xunit;

namespace Waypoint.Goals.Tests;

public class HeuristicPlanDraftGeneratorTests
{
    private readonly HeuristicPlanDraftGenerator _generator = new();

    private static DreamSummary MakeDream(
        string title = "Cut waste for small manufacturers",
        string? outcome = null, string? whoItHelps = null, string? problem = null, bool isBusinessShaped = false) =>
        new(Guid.NewGuid(), Guid.NewGuid(), title,
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

    // --- Regression coverage for the template-composition bug found in live testing ---
    //
    // The five-year vision and three-year direction used to splice the user's own free-text
    // Outcome/WhoItHelps answers directly into the middle of a sentence, as if they were
    // guaranteed to be a lowercase noun phrase continuing "...has become {outcome}." That
    // assumption doesn't hold - a user answering "what does success look like?" is just as
    // likely to write a complete, capitalized sentence as a noun phrase. When they do, the old
    // template produced exactly this, verbatim, from a real local test run:
    //
    //   "In five years, \"A studio that helps small shop owners run their business without
    //   dread\" has become Owners spend under 30 minutes a week on admin instead of a full day."
    //
    // - a run-on sentence with a capital letter appearing mid-clause. The fix changes the
    // composition boundary itself (introduce the user's own words with a colon and quote them,
    // the same pattern already used for Problem/OneYearGoal below) rather than patching this one
    // string - these tests check the boundary is safe for many different shapes of input, not
    // just the one exact sentence that happened to surface the bug.

    [Fact]
    public void Reproduces_and_fixes_the_exact_malformed_output_found_in_live_testing()
    {
        var dream = MakeDream(
            title: "A studio that helps small shop owners run their business without dread",
            outcome: "Owners spend under 30 minutes a week on admin instead of a full day.");

        var result = _generator.Generate(dream);

        // The old, broken shape: the outcome's capitalized subject ("Owners") glued directly
        // onto "has become" with no punctuation between them.
        result.FiveYearVision.Should().NotContain("has become Owners");
        // The fixed shape: the user's own sentence is quoted, not grammatically absorbed.
        result.FiveYearVision.Should().Be(
            "In five years, \"A studio that helps small shop owners run their business without dread\" " +
            "has become real: \"Owners spend under 30 minutes a week on admin instead of a full day\".");
    }

    [Theory]
    [InlineData("Owners spend under 30 minutes a week on admin instead of a full day.")] // full sentence, capitalized
    [InlineData("a thriving studio with five long-term clients")] // lowercase noun phrase
    [InlineData("People trust it enough to recommend it to a friend.")] // full sentence, different subject
    [InlineData("the go-to tool for small shop owners")] // lowercase noun phrase, different shape
    public void Five_year_vision_never_produces_an_unquoted_grammatical_splice_regardless_of_outcome_shape(
        string outcome)
    {
        var result = _generator.Generate(MakeDream(outcome: outcome));

        // Whatever the user wrote, it must appear as a direct, quoted citation - never spliced
        // in raw as if it continued "has become" grammatically.
        result.FiveYearVision.Should().Contain($"\"{outcome.TrimEnd('.')}\"");
        result.FiveYearVision.Should().NotContain($"has become {outcome}");
    }

    [Theory]
    [InlineData("Small retail and service shop owners with 1-5 employees.")] // full sentence
    [InlineData("small manufacturing shop owners")] // lowercase noun phrase
    [InlineData("Freelance designers who bill by the hour.")] // full sentence, different subject
    public void Three_year_direction_never_produces_an_unquoted_grammatical_splice_regardless_of_who_it_helps_shape(
        string whoItHelps)
    {
        var result = _generator.Generate(MakeDream(whoItHelps: whoItHelps));

        result.ThreeYearDirection.Should().Contain($"\"{whoItHelps.TrimEnd('.')}\"");
        result.ThreeYearDirection.Should().NotContain($"delivering for {whoItHelps}");
    }

    [Theory]
    [InlineData("Cut waste for small manufacturers")]
    [InlineData("A studio that helps small shop owners run their business without dread")]
    [InlineData("Write a book about growing up on a farm")]
    public void Fallback_phrasing_reads_naturally_across_different_dream_titles_when_outcome_and_who_it_helps_are_empty(
        string title)
    {
        var result = _generator.Generate(MakeDream(title: title, outcome: null, whoItHelps: null));

        result.FiveYearVision.Should().Be(
            $"In five years, \"{title}\" has become a version of this dream you're proud of.");
        result.ThreeYearDirection.Should().Be(
            "By year three, you have a track record of actually delivering for the people this is meant for.");
    }

    [Fact]
    public void Long_outcome_is_truncated_with_an_ellipsis_inside_the_quote_rather_than_mid_sentence()
    {
        var longOutcome = string.Join(" ", Enumerable.Repeat("word", 60)); // well over the 140-char Trim() limit

        var result = _generator.Generate(MakeDream(outcome: longOutcome));

        // Truncating inside a direct quote always reads fine ("...: "word word word…"."); the old
        // template would have truncated mid-continuation-clause, which could read as nonsense.
        result.FiveYearVision.Should().Contain("…\".");
    }
}
