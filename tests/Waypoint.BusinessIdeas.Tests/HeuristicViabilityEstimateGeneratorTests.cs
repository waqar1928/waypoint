using FluentAssertions;
using Waypoint.BusinessIdeas.Application;
using Waypoint.BusinessIdeas.Domain;
using Xunit;

namespace Waypoint.BusinessIdeas.Tests;

public class HeuristicViabilityEstimateGeneratorTests
{
    private readonly HeuristicViabilityEstimateGenerator _generator = new();

    [Fact]
    public void Empty_profile_scores_zero_and_still_returns_a_usable_draft()
    {
        var idea = BusinessIdea.Create(Guid.NewGuid(), Guid.NewGuid());

        var draft = _generator.Generate(idea, "My Dream");

        draft.ViabilityEstimate.Should().Be(0);
        draft.StrongAssumptions.Should().ContainSingle();
        draft.WeakAssumptions.Should().NotBeEmpty();
        draft.RecommendedExperiments.Should().NotBeEmpty();
    }

    [Fact]
    public void Core_triad_plus_two_viability_fields_earns_the_full_bonus()
    {
        var idea = BusinessIdea.Create(Guid.NewGuid(), Guid.NewGuid());
        idea.Problem = "Shops waste material";
        idea.Customer = "Small manufacturers";
        idea.ValueProposition = "We help them see the waste";
        idea.Pricing = "$99/month";
        idea.Market = "Local manufacturing shops";

        var draft = _generator.Generate(idea, "My Dream");

        // 5 of 14 fields filled -> ~21 completeness points, +20 core triad, +20 viability signals.
        draft.ViabilityEstimate.Should().BeGreaterThan(50);
        draft.StrongAssumptions.Should().HaveCount(5);
    }

    [Fact]
    public void Fully_filled_profile_caps_at_one_hundred()
    {
        var idea = BusinessIdea.Create(Guid.NewGuid(), Guid.NewGuid());
        idea.Problem = "p";
        idea.Customer = "c";
        idea.ValueProposition = "v";
        idea.Solution = "s";
        idea.BusinessModel = "b";
        idea.Market = "m";
        idea.Competitors = "co";
        idea.Pricing = "pr";
        idea.Marketing = "mk";
        idea.Sales = "sa";
        idea.Operations = "o";
        idea.Technology = "t";
        idea.FinancialAssumptions = "f";
        idea.Risks = "r";

        var draft = _generator.Generate(idea, "My Dream");

        draft.ViabilityEstimate.Should().Be(100);
        draft.WeakAssumptions.Should().BeEmpty();
        draft.Unknowns.Should().BeEmpty();
    }
}
