namespace Waypoint.Dreams.Application;

/// <summary>
/// Deterministic, non-AI Dream Direction generator (see IDreamDirectionGenerator).
/// Combines the user's own words from Discovery into template statements —
/// intentionally rough; the UI frames these as drafts to edit, not answers.
/// </summary>
public sealed class HeuristicDreamDirectionGenerator : IDreamDirectionGenerator
{
    /// <summary>
    /// Used to top up the list to at least 3 directions when Discovery answers were mostly
    /// skipped — always distinct entries, never the same card repeated.
    /// </summary>
    private static readonly IReadOnlyList<DreamDirectionDto> FallbackDirections =
    [
        new(
            DirectionStatement: "Keep exploring — nothing here has to be decided yet.",
            WhyItFits: "You can always come back to Discovery once you've had a chance to think.",
            SkillsPresent: "Whatever you bring to this is enough to start a conversation.",
            SkillsNeeded: "More clarity usually comes from doing something small, not from more thinking.",
            Opportunities: "Most dreams start out vague — that's normal, not a problem.",
            Challenges: "The main risk right now is waiting for certainty before you start.",
            FirstExperiment: "Talk to one person this week about what's been on your mind.",
            SuggestedBusinessShaped: false),
        new(
            DirectionStatement: "Revisit this once something's been on your mind for a few days.",
            WhyItFits: "Some dreams need a bit of time to surface — that's normal too.",
            SkillsPresent: "You don't need to have this figured out to start paying attention.",
            SkillsNeeded: "Mostly just noticing what keeps pulling at your attention.",
            Opportunities: "The next thing that annoys or excites you is worth writing down.",
            Challenges: "It's easy to let a vague feeling pass without ever naming it.",
            FirstExperiment: "Keep a running note for a week of anything that sticks with you.",
            SuggestedBusinessShaped: false),
        new(
            DirectionStatement: "Talk to someone before deciding on a direction.",
            WhyItFits: "Sometimes clarity comes from a conversation, not more solo thinking.",
            SkillsPresent: "You clearly care enough to be here — that's a real starting point.",
            SkillsNeeded: "Just someone willing to listen and ask you honest questions.",
            Opportunities: "An outside perspective often surfaces things you can't see yourself.",
            Challenges: "It can feel exposing to talk about something this undefined.",
            FirstExperiment: "Ask one person you trust what they think you're good at.",
            SuggestedBusinessShaped: false),
    ];

    public IReadOnlyList<DreamDirectionDto> Generate(DiscoveryAnswers a)
    {
        var directions = new List<DreamDirectionDto>();

        if (!string.IsNullOrWhiteSpace(a.ProblemToSolve))
        {
            directions.Add(new DreamDirectionDto(
                DirectionStatement: $"Take on the problem you named: \"{Trim(a.ProblemToSolve)}\".",
                WhyItFits: BuildWhyItFits(a, a.SpendTimeDoing, "you said you'd spend your time on this if you couldn't fail"),
                SkillsPresent: SummarizeOrFallback(a.DoWithoutPay, "The things you already do without being asked are a starting signal — list them out."),
                SkillsNeeded: "Whatever's missing usually becomes clear once you talk to the first few people affected by this problem.",
                Opportunities: "Nobody's solved this well yet, or you wouldn't still be thinking about it.",
                Challenges: "Turning a problem you care about into something you actually build is the hard part — start small.",
                FirstExperiment: "Have three conversations this week with people who deal with this problem regularly.",
                SuggestedBusinessShaped: !string.IsNullOrWhiteSpace(a.ProblemsNoticed)));
        }

        if (!string.IsNullOrWhiteSpace(a.WhatWouldYouChange))
        {
            directions.Add(new DreamDirectionDto(
                DirectionStatement: $"Work on the change you described: \"{Trim(a.WhatWouldYouChange)}\".",
                WhyItFits: BuildWhyItFits(a, a.ImpactOnOthers, "it lines up with the kind of impact you said you want to have"),
                SkillsPresent: SummarizeOrFallback(a.AdmiredWork, "Think about what draws you to the people/work you admire — that's often a skill you already have or want."),
                SkillsNeeded: "Talk to someone already working on something adjacent to this change.",
                Opportunities: "Small, local versions of big changes are usually where people actually start.",
                Challenges: "Big changes can feel too large to start — pick the smallest piece of it.",
                FirstExperiment: "Write down the smallest version of this change you could attempt in the next 30 days.",
                SuggestedBusinessShaped: false));
        }

        if (!string.IsNullOrWhiteSpace(a.AdmiredWork) || !string.IsNullOrWhiteSpace(a.ProudInFiveYears))
        {
            directions.Add(new DreamDirectionDto(
                DirectionStatement: BuildAdmiredDirection(a),
                WhyItFits: BuildWhyItFits(a, a.FutureExperience, "it connects to the kind of experience you want to be able to talk about later"),
                SkillsPresent: SummarizeOrFallback(a.TypicalWeek, "Look at how you already spend your time — some of this may already be underway."),
                SkillsNeeded: "Whoever you admire started somewhere too — see if you can find out how.",
                Opportunities: "Being early and curious about something usually counts for more than people think.",
                Challenges: "Admiration alone isn't a plan — the next step is finding your own angle on it.",
                FirstExperiment: "Reach out to one person doing work you admire and ask how they started.",
                SuggestedBusinessShaped: false));
        }

        if (!string.IsNullOrWhiteSpace(a.IfMoneyWerentFactor))
        {
            directions.Add(new DreamDirectionDto(
                DirectionStatement: $"Explore what you said you'd do if money weren't a factor: \"{Trim(a.IfMoneyWerentFactor)}\".",
                WhyItFits: BuildWhyItFits(a, a.RegretNeverTrying, "it's close to the thing you said you'd regret never trying"),
                SkillsPresent: SummarizeOrFallback(a.DrainingWork, "Notice what this direction does NOT have in common with the work that drains you — that's a good sign."),
                SkillsNeeded: "Whatever gap exists here is usually smaller once you start than it looks from the outside.",
                Opportunities: "Things people would do without being paid often turn out to be things other people will pay for.",
                Challenges: "It's easy to romanticize this direction — test it with something real before committing.",
                FirstExperiment: "Spend two hours this week actually doing this, and notice how it feels.",
                SuggestedBusinessShaped: false));
        }

        foreach (var fallback in FallbackDirections)
        {
            if (directions.Count >= 3) break;
            directions.Add(fallback);
        }

        return directions.Take(5).ToList();
    }

    private static string BuildAdmiredDirection(DiscoveryAnswers a)
    {
        if (!string.IsNullOrWhiteSpace(a.ProudInFiveYears))
        {
            return $"Build toward what would make you proud in five years: \"{Trim(a.ProudInFiveYears)}\".";
        }
        return $"Follow what draws you to the work you admire: \"{Trim(a.AdmiredWork)}\".";
    }

    private static string BuildWhyItFits(DiscoveryAnswers a, string? supportingAnswer, string reason) =>
        string.IsNullOrWhiteSpace(supportingAnswer)
            ? $"This came directly from what you shared in Discovery ({FeelingsSummary(a)})."
            : $"This might fit because {reason}.";

    private static string FeelingsSummary(DiscoveryAnswers a) =>
        a.Feelings.Count > 0 ? string.Join(", ", a.Feelings) : "your own words";

    private static string SummarizeOrFallback(string? answer, string fallback) =>
        string.IsNullOrWhiteSpace(answer) ? fallback : $"You mentioned: \"{Trim(answer)}\" — that's worth building on.";

    private static string Trim(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var t = text.Trim().TrimEnd('.', '!', '?');
        return t.Length > 140 ? t[..140].TrimEnd() + "…" : t;
    }
}
