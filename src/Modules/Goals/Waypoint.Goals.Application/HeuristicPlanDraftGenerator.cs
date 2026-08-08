using Waypoint.Common;

namespace Waypoint.Goals.Application;

/// <summary>Deterministic, non-AI plan draft generator (see IPlanDraftGenerator).</summary>
public sealed class HeuristicPlanDraftGenerator : IPlanDraftGenerator
{
    public PlanDraftDto Generate(DreamSummary dream)
    {
        var outcome = string.IsNullOrWhiteSpace(dream.Outcome)
            ? "a version of this dream you're proud of"
            : Trim(dream.Outcome);

        var whoItHelps = string.IsNullOrWhiteSpace(dream.WhoItHelps)
            ? "the people this is meant for"
            : Trim(dream.WhoItHelps);

        var fiveYearVision = $"In five years, \"{Trim(dream.Title)}\" has become {outcome}.";

        var threeYearDirection = $"By year three, you have a track record of actually delivering for {whoItHelps}.";

        var oneYearGoal = string.IsNullOrWhiteSpace(dream.Problem)
            ? $"In the next year, take \"{Trim(dream.Title)}\" from an idea to something real."
            : $"In the next year, build a first real answer to: \"{Trim(dream.Problem)}\".";

        var ninetyDayMission = dream.IsBusinessShaped
            ? $"Validate the core idea behind \"{Trim(dream.Title)}\" with real conversations and a small test."
            : $"Take the first concrete steps on \"{Trim(dream.Title)}\" and see what you learn.";

        return new PlanDraftDto(fiveYearVision, threeYearDirection, oneYearGoal, ninetyDayMission);
    }

    private static string Trim(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var t = text.Trim().TrimEnd('.', '!', '?');
        return t.Length > 140 ? t[..140].TrimEnd() + "…" : t;
    }
}
