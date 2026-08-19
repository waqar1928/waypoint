using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Application.GetNextBestAction;

/// <summary>
/// Picks which open action to recommend next and explains why, using the signals every action
/// already carries (priority, difficulty, impact, due date) instead of requiring the user to
/// manually flag one. A manual pin (ActionItem.IsNextBestAction, set via SetNextBestActionCommand)
/// is still respected as an explicit override by the caller - this selector is only the computed
/// fallback used when nothing is pinned, or the pinned action was completed/cancelled. Nothing
/// here is persisted; it's recomputed on every read, so the recommendation always reflects the
/// current state of the action list.
///
/// This is deliberately a plain, deterministic scoring function, not an AI call - see
/// docs/BRAND_VOICE.md and the AI module's doc comments on which features are genuinely
/// AI-backed (only Drevia Coach) versus heuristic. A "why" a user can hold you to only works if
/// it's the same reason every time for the same inputs.
/// </summary>
public static class NextBestActionSelector
{
    public static (ActionItem Action, string Rationale)? SelectFrom(IReadOnlyList<ActionItem> actions, DateOnly today)
    {
        var candidates = actions.Where(a => a.Status is ActionStatus.NotStarted or ActionStatus.InProgress).ToList();
        if (candidates.Count == 0)
        {
            return null;
        }

        var winner = candidates
            .OrderByDescending(a => Score(a, today))
            .ThenBy(a => a.DueDate ?? DateOnly.MaxValue)
            .ThenBy(a => a.CreatedAt)
            .First();

        return (winner, Explain(winner, today));
    }

    /// <summary>
    /// Priority and impact carry equal, heaviest weight since both directly answer "does this
    /// matter." Ease is a much lighter tiebreaker, not a primary factor - it should only decide
    /// between two actions that already matter about the same amount, never let a trivial task
    /// outrank an important one. Urgency (due date) can add enough to push a due-soon action
    /// ahead of a slightly-higher-impact one that isn't time-bound, which matches how a person
    /// would actually decide between them.
    /// </summary>
    private static int Score(ActionItem a, DateOnly today)
    {
        var priorityScore = a.Priority switch { ActionPriority.High => 3, ActionPriority.Medium => 2, _ => 1 };
        var impactScore = a.ExpectedImpact switch { ActionImpact.High => 3, ActionImpact.Medium => 2, _ => 1 };
        var easeScore = a.Difficulty switch { ActionDifficulty.Easy => 1, ActionDifficulty.Medium => 0, _ => -1 };
        var urgencyScore = a.DueDate switch
        {
            { } due when due < today => 4,
            { } due when due <= today.AddDays(7) => 2,
            _ => 0,
        };

        return priorityScore * 3 + impactScore * 3 + easeScore + urgencyScore;
    }

    private static string Explain(ActionItem a, DateOnly today)
    {
        var reasons = new List<string>();

        if (a.DueDate is { } due && due < today)
        {
            reasons.Add("it's overdue");
        }
        else if (a.DueDate is { } dueSoon && dueSoon <= today.AddDays(7))
        {
            reasons.Add("it's due soon");
        }

        if (a.Priority == ActionPriority.High)
        {
            reasons.Add("it's high priority");
        }

        if (a.ExpectedImpact == ActionImpact.High)
        {
            reasons.Add("it's likely to move things forward the most");
        }

        if (reasons.Count == 0 && a.Difficulty == ActionDifficulty.Easy)
        {
            reasons.Add("it's a quick, easy win");
        }

        return reasons.Count switch
        {
            0 => "This is the next open action on your list.",
            1 => $"This is next because {reasons[0]}.",
            _ => $"This is next because {string.Join(" and ", reasons.Take(2))}.",
        };
    }
}
