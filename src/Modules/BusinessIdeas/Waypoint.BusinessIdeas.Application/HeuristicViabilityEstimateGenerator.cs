using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application;

/// <summary>
/// Deterministic, non-AI stand-in for a real viability model (see IViabilityEstimateGenerator).
/// Scores completeness and highlights which of the core signals (problem/customer/value
/// proposition, and pricing/market/competitors/financials) are filled in versus missing, then
/// turns the gaps into plain-language "weak assumptions" / "unknowns" and a couple of concrete
/// next experiments. It has no opinion about whether the idea is actually good — only about how
/// much of the picture the user has filled in so far.
/// </summary>
public sealed class HeuristicViabilityEstimateGenerator : IViabilityEstimateGenerator
{
    private static readonly (string Label, Func<BusinessIdea, string?> Get)[] CoreFields =
    [
        ("a clear problem statement", i => i.Problem),
        ("a defined customer", i => i.Customer),
        ("a value proposition", i => i.ValueProposition),
    ];

    private static readonly (string Label, Func<BusinessIdea, string?> Get)[] ViabilityFields =
    [
        ("pricing", i => i.Pricing),
        ("your market", i => i.Market),
        ("your competitors", i => i.Competitors),
        ("your financial assumptions", i => i.FinancialAssumptions),
    ];

    private static readonly (string Label, Func<BusinessIdea, string?> Get)[] OtherFields =
    [
        ("your solution", i => i.Solution),
        ("your business model", i => i.BusinessModel),
        ("marketing", i => i.Marketing),
        ("sales", i => i.Sales),
        ("operations", i => i.Operations),
        ("technology", i => i.Technology),
        ("risks", i => i.Risks),
    ];

    public ViabilityEstimateDraft Generate(BusinessIdea idea, string dreamTitle)
    {
        var allFields = CoreFields.Concat(ViabilityFields).Concat(OtherFields).ToArray();
        var filledCount = allFields.Count(f => !string.IsNullOrWhiteSpace(f.Get(idea)));
        var completeness = (double)filledCount / allFields.Length;

        var score = (int)Math.Round(completeness * 60);
        if (CoreFields.All(f => !string.IsNullOrWhiteSpace(f.Get(idea))))
        {
            score += 20;
        }
        if (ViabilityFields.Count(f => !string.IsNullOrWhiteSpace(f.Get(idea))) >= 2)
        {
            score += 20;
        }
        score = Math.Clamp(score, 0, 100);

        var strong = allFields
            .Where(f => !string.IsNullOrWhiteSpace(f.Get(idea)))
            .Select(f => $"You have {f.Label}: “{Trim(f.Get(idea))}”")
            .ToList();

        var weak = ViabilityFields
            .Where(f => string.IsNullOrWhiteSpace(f.Get(idea)))
            .Select(f => $"You haven't worked out {f.Label} yet. That makes it hard to tell if this can make money.")
            .ToList();

        var unknowns = CoreFields.Concat(OtherFields)
            .Where(f => string.IsNullOrWhiteSpace(f.Get(idea)))
            .Select(f => $"Still unknown: {f.Label}.")
            .ToList();

        var experiments = new List<string>();
        if (string.IsNullOrWhiteSpace(idea.Customer))
        {
            experiments.Add($"Talk to 5 people who might be the customer for “{Trim(dreamTitle)}” and see if the problem is real for them.");
        }
        if (string.IsNullOrWhiteSpace(idea.Pricing))
        {
            experiments.Add("Ask 3 potential customers what they'd expect to pay, before you set a price.");
        }
        if (string.IsNullOrWhiteSpace(idea.Competitors))
        {
            experiments.Add("Find 3 ways people currently solve this problem without you, even informal ones.");
        }
        if (experiments.Count == 0)
        {
            experiments.Add($"Run a small, cheap test of “{Trim(dreamTitle)}” with real people before investing more.");
        }

        if (strong.Count == 0)
        {
            strong.Add("Nothing filled in yet. This estimate will get more useful as you fill out the profile.");
        }

        return new ViabilityEstimateDraft(score, strong, weak, unknowns, experiments);
    }

    private static string Trim(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var t = text.Trim().TrimEnd('.', '!', '?');
        return t.Length > 140 ? t[..140].TrimEnd() + "…" : t;
    }
}
