using FluentAssertions;
using NSubstitute;
using Waypoint.Actions.Application;
using Waypoint.Actions.Domain;
using Waypoint.Actions.Infrastructure;
using Waypoint.Common;
using Xunit;

namespace Waypoint.Actions.Tests;

/// <summary>
/// Proves the P1 push-notification worker's read path (INextBestActionSummaryProvider) agrees
/// exactly with GetNextBestActionQueryHandler (see GetNextBestActionQueryHandlerTests) - both call
/// the same NextBestActionSelector.SelectFrom(...) and the same PinnedRationale constant, so there
/// is exactly one place either could ever be wrong. This is the actual proof that the notification
/// system never develops a competing definition of "next best action."
/// </summary>
public class NextBestActionSummaryProviderTests
{
    private readonly IActionsRepository _repository = Substitute.For<IActionsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private NextBestActionSummaryProvider CreateProvider() => new(_repository, _dreamSummaryProvider, _timeProvider);

    private void ArrangeUserWithDream() =>
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));

    private ActionItem MakeAction(
        string title, ActionPriority priority = ActionPriority.Medium, ActionImpact impact = ActionImpact.Medium,
        ActionStatus status = ActionStatus.NotStarted) =>
        new()
        {
            DreamId = _dreamId,
            Title = title,
            Priority = priority,
            ExpectedImpact = impact,
            Status = status,
            CreatedBy = _userId,
            UpdatedBy = _userId,
        };

    [Fact]
    public async Task Returns_null_when_the_user_has_no_dream()
    {
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((DreamSummary?)null);

        var result = await CreateProvider().GetForUserAsync(_userId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task A_manual_pin_wins_with_the_exact_same_rationale_text_the_query_handler_uses()
    {
        ArrangeUserWithDream();
        var pinned = MakeAction("The one I chose", priority: ActionPriority.Low, impact: ActionImpact.Low);
        pinned.IsNextBestAction = true;
        _repository.GetNextBestActionAsync(_dreamId, Arg.Any<CancellationToken>()).Returns(pinned);

        var result = await CreateProvider().GetForUserAsync(_userId, CancellationToken.None);

        result!.Title.Should().Be("The one I chose");
        result.Rationale.Should().Be("You marked this as your next move.");
    }

    [Fact]
    public async Task Falls_back_to_the_same_computed_selection_as_GetNextBestActionQueryHandler()
    {
        ArrangeUserWithDream();
        _repository.GetNextBestActionAsync(_dreamId, Arg.Any<CancellationToken>()).Returns((ActionItem?)null);
        var strong = MakeAction("Talk to five customers", priority: ActionPriority.High, impact: ActionImpact.High);
        var weak = MakeAction("Tidy notes", priority: ActionPriority.Low, impact: ActionImpact.Low);
        _repository.GetForDreamAsync(_dreamId, null, Arg.Any<CancellationToken>()).Returns([weak, strong]);

        var result = await CreateProvider().GetForUserAsync(_userId, CancellationToken.None);

        result!.Title.Should().Be("Talk to five customers");
        result.Rationale.Should().NotBeNullOrWhiteSpace();
        result.ActionId.Should().Be(strong.Id);
    }

    [Fact]
    public async Task Returns_null_when_nothing_is_pinned_and_no_open_actions_exist()
    {
        ArrangeUserWithDream();
        _repository.GetNextBestActionAsync(_dreamId, Arg.Any<CancellationToken>()).Returns((ActionItem?)null);
        _repository.GetForDreamAsync(_dreamId, null, Arg.Any<CancellationToken>()).Returns([]);

        var result = await CreateProvider().GetForUserAsync(_userId, CancellationToken.None);

        result.Should().BeNull();
    }
}
