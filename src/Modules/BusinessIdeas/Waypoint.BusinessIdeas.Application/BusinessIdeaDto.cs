using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application;

public sealed record BusinessIdeaDto(
    Guid Id,
    string? Problem,
    string? Customer,
    string? ValueProposition,
    string? Solution,
    string? BusinessModel,
    string? Market,
    string? Competitors,
    string? Pricing,
    string? Marketing,
    string? Sales,
    string? Operations,
    string? Technology,
    string? FinancialAssumptions,
    string? Risks)
{
    public static BusinessIdeaDto From(BusinessIdea idea) => new(
        idea.Id, idea.Problem, idea.Customer, idea.ValueProposition, idea.Solution, idea.BusinessModel,
        idea.Market, idea.Competitors, idea.Pricing, idea.Marketing, idea.Sales, idea.Operations,
        idea.Technology, idea.FinancialAssumptions, idea.Risks);
}

public sealed record BusinessValidationDto(
    Guid Id,
    int? ViabilityEstimate,
    IReadOnlyList<string> StrongAssumptions,
    IReadOnlyList<string> WeakAssumptions,
    IReadOnlyList<string> Unknowns,
    IReadOnlyList<string> RecommendedExperiments,
    bool GeneratedByAi,
    DateTimeOffset CreatedAt)
{
    public static BusinessValidationDto From(BusinessValidation v) => new(
        v.Id, v.ViabilityEstimate, v.StrongAssumptions, v.WeakAssumptions, v.Unknowns,
        v.RecommendedExperiments, v.GeneratedByAi, v.CreatedAt);
}
