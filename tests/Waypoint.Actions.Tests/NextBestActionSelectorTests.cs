using FluentAssertions;
using Waypoint.Actions.Application.GetNextBestAction;
using Waypoint.Actions.Domain;
using Xunit;

namespace Waypoint.Actions.Tests;

public class NextBestActionSelectorTests
{
    private static readonly Guid DreamId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 6, 15);

    private static ActionItem MakeAction(
        string title, ActionPriority priority = ActionPriority.Medium,
        ActionDifficulty difficulty = ActionDifficulty.Medium, ActionImpact impact = ActionImpact.Medium,
        DateOnly? dueDate = null, ActionStatus status = ActionStatus.NotStarted) =>
        new ActionItem
        {
            DreamId = DreamId,
            Title = title,
            Priority = priority,
            Difficulty = difficulty,
            ExpectedImpact = impact,
            DueDate = dueDate,
            Status = status,
            CreatedBy = UserId,
            UpdatedBy = UserId,
        };

    [Fact]
    public void Returns_null_when_there_are_no_open_actions()
    {
        var completed = MakeAction("Done", status: ActionStatus.Completed);

        var result = NextBestActionSelector.SelectFrom([completed], Today);

        result.Should().BeNull();
    }

    [Fact]
    public void Ignores_completed_blocked_and_cancelled_actions()
    {
        var open = MakeAction("The only real option", priority: ActionPriority.Low, impact: ActionImpact.Low);
        var completed = MakeAction("Done", priority: ActionPriority.High, impact: ActionImpact.High, status: ActionStatus.Completed);
        var blocked = MakeAction("Blocked", priority: ActionPriority.High, impact: ActionImpact.High, status: ActionStatus.Blocked);
        var cancelled = MakeAction("Cancelled", priority: ActionPriority.High, impact: ActionImpact.High, status: ActionStatus.Cancelled);

        var result = NextBestActionSelector.SelectFrom([completed, blocked, cancelled, open], Today);

        result!.Value.Action.Should().Be(open);
    }

    [Fact]
    public void Prefers_high_priority_and_high_impact_over_low()
    {
        var strong = MakeAction("Talk to five customers", priority: ActionPriority.High, impact: ActionImpact.High);
        var weak = MakeAction("Tidy up notes", priority: ActionPriority.Low, impact: ActionImpact.Low);

        var result = NextBestActionSelector.SelectFrom([weak, strong], Today);

        result!.Value.Action.Should().Be(strong);
        result.Value.Rationale.Should().Contain("high priority");
    }

    [Fact]
    public void An_overdue_action_can_outrank_a_higher_impact_one_that_is_not_time_bound()
    {
        var overdue = MakeAction(
            "Send the overdue invoice", priority: ActionPriority.Medium, impact: ActionImpact.Medium,
            dueDate: Today.AddDays(-3));
        var notUrgent = MakeAction("Someday idea", priority: ActionPriority.Medium, impact: ActionImpact.High);

        var result = NextBestActionSelector.SelectFrom([notUrgent, overdue], Today);

        result!.Value.Action.Should().Be(overdue);
        result.Value.Rationale.Should().Contain("overdue");
    }

    [Fact]
    public void Uses_ease_only_as_a_tiebreaker_between_actions_that_matter_about_the_same_amount()
    {
        var easy = MakeAction("Quick email", priority: ActionPriority.Medium, impact: ActionImpact.Medium, difficulty: ActionDifficulty.Easy);
        var hard = MakeAction("Big proposal", priority: ActionPriority.Medium, impact: ActionImpact.Medium, difficulty: ActionDifficulty.Hard);

        var result = NextBestActionSelector.SelectFrom([hard, easy], Today);

        result!.Value.Action.Should().Be(easy);
    }

    [Fact]
    public void Same_score_breaks_ties_by_earliest_due_date_then_oldest_created()
    {
        var soonerDue = MakeAction("Due first", dueDate: Today.AddDays(20));
        var laterDue = MakeAction("Due later", dueDate: Today.AddDays(40));

        var result = NextBestActionSelector.SelectFrom([laterDue, soonerDue], Today);

        result!.Value.Action.Should().Be(soonerDue);
    }

    [Fact]
    public void Rationale_falls_back_to_a_plain_sentence_when_nothing_stands_out()
    {
        var plain = MakeAction("Just the only thing on the list");

        var result = NextBestActionSelector.SelectFrom([plain], Today);

        result!.Value.Rationale.Should().Be("This is the next open action on your list.");
    }
}
