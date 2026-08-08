using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Application;

public sealed record DreamDto(
    Guid Id,
    string Title,
    DreamStage Stage,
    bool IsBusinessShaped,
    string Statement,
    string? Purpose,
    string? WhoItHelps,
    string? Problem,
    string? Outcome,
    string? Motivation,
    string? Impact);

public sealed record DreamDirectionDto(
    string DirectionStatement,
    string WhyItFits,
    string SkillsPresent,
    string SkillsNeeded,
    string Opportunities,
    string Challenges,
    string FirstExperiment,
    bool SuggestedBusinessShaped);
