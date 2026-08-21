using Waypoint.Common;

namespace Waypoint.Goals.Application;

/// <summary>Deterministic, non-AI plan draft generator (see IPlanDraftGenerator).</summary>
public sealed class HeuristicPlanDraftGenerator : IPlanDraftGenerator
{
    public PlanDraftDto Generate(DreamSummary dream)
    {
        // Outcome and WhoItHelps are free text the user wrote to answer a specific question
        // ("what does success look like," "who is this for") - there's no guarantee they wrote a
        // noun phrase rather than a full sentence, so splicing them in as a grammatical
        // continuation of our own sentence ("...has become {outcome}.") produces run-on, oddly-
        // capitalized nonsense whenever the user's answer happens to be a complete sentence of
        // its own (e.g. "Owners spend under 30 minutes a week on admin instead of a full day."
        // spliced after "has become" reads as "...has become Owners spend..."). The fix is the
        // same pattern already used a few lines down for Problem: introduce the user's own words
        // with a colon and quote them directly rather than grammatically absorbing them. A direct
        // quote is correct regardless of what part of speech the user actually wrote, and it stays
        // correct even if Trim() truncates a long answer with an ellipsis - truncating mid-quote
        // reads fine, truncating mid-continuation-clause would not have.
        var fiveYearVision = string.IsNullOrWhiteSpace(dream.Outcome)
            ? $"In five years, \"{Trim(dream.Title)}\" has become a version of this dream you're proud of."
            : $"In five years, \"{Trim(dream.Title)}\" has become real: \"{Trim(dream.Outcome)}\".";

        var threeYearDirection = string.IsNullOrWhiteSpace(dream.WhoItHelps)
            ? "By year three, you have a track record of actually delivering for the people this is meant for."
            : $"By year three, you have a track record of actually delivering for the people this is for: \"{Trim(dream.WhoItHelps)}\".";

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
