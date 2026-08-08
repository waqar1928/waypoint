namespace Waypoint.Dreams.Application;

/// <summary>
/// The Stage 1 (Discover) and Stage 2 (Discover Your Dream) answers from
/// docs/02-user-journey.md. Collected client-side across the onboarding
/// wizard and submitted once to generate Dream Directions — not persisted
/// on its own (see docs/09-phased-plan.md Phase 2 scope notes); only the
/// Dream the user actually selects gets saved.
/// </summary>
public sealed record DiscoveryAnswers
{
    // Stage 1 — Who Are You?
    public string? TypicalWeek { get; init; }
    public IReadOnlyList<string> Feelings { get; init; } = [];
    public string? DoWithoutPay { get; init; }
    public string? ProblemsNoticed { get; init; }
    public string? AdmiredWork { get; init; }
    public string? IfMoneyWerentFactor { get; init; }
    public string? DriftedAwayFrom { get; init; }
    public string? DrainingWork { get; init; }
    public string? FutureExperience { get; init; }

    // Stage 2 — Discover Your Dream
    public string? WhatWouldYouChange { get; init; }
    public string? ProblemToSolve { get; init; }
    public string? SpendTimeDoing { get; init; }
    public string? ProudInFiveYears { get; init; }
    public string? RegretNeverTrying { get; init; }
    public string? ImpactOnOthers { get; init; }
}
