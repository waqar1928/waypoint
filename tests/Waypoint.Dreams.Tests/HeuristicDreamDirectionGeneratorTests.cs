using FluentAssertions;
using Waypoint.Dreams.Application;
using Xunit;

namespace Waypoint.Dreams.Tests;

public class HeuristicDreamDirectionGeneratorTests
{
    private readonly HeuristicDreamDirectionGenerator _generator = new();

    [Fact]
    public void Generates_at_least_three_directions_even_with_minimal_answers()
    {
        var result = _generator.Generate(new DiscoveryAnswers());

        result.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Never_generates_more_than_five_directions()
    {
        var answers = new DiscoveryAnswers
        {
            ProblemToSolve = "Solve X",
            SpendTimeDoing = "Doing Y",
            WhatWouldYouChange = "Change Z",
            ImpactOnOthers = "Help people",
            AdmiredWork = "Admired work",
            ProudInFiveYears = "Proud thing",
            IfMoneyWerentFactor = "Money-free thing",
            RegretNeverTrying = "Regret thing",
        };

        var result = _generator.Generate(answers);

        result.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public void Reflects_the_users_own_words_in_generated_directions()
    {
        var answers = new DiscoveryAnswers
        {
            ProblemToSolve = "Small manufacturers waste a lot of material",
        };

        var result = _generator.Generate(answers);

        result.Should().Contain(d => d.DirectionStatement.Contains("Small manufacturers waste a lot of material"));
    }

    [Fact]
    public void Marks_business_shaped_suggestion_when_problems_noticed_is_present()
    {
        var answers = new DiscoveryAnswers
        {
            ProblemToSolve = "A real problem",
            ProblemsNoticed = "Something worth building a business around",
        };

        var result = _generator.Generate(answers);

        result.Should().Contain(d => d.SuggestedBusinessShaped);
    }
}
