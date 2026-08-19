using FluentAssertions;
using NSubstitute;
using Waypoint.Actions.Application;
using Waypoint.Actions.Application.GetNextBestAction;
using Waypoint.Actions.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.Actions.Tests;

public class GetNextBestActionQueryHandlerTests
{
    private readonly IActionsRepository _repository = Substitute.For<IActionsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private GetNextBestActionQueryHandler CreateHandler() =>
        new(_repository, _dreamSummaryProvider, _currentUser, _timeProvider);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

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
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((DreamSummary?)null);

        var result = await CreateHandler().Handle(new GetNextBestActionQuery(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task A_manual_pin_always_wins_over_the_computed_pick()
    {
        ArrangeSignedInUserWithDream();
        var pinned = MakeAction("The one I chose", priority: ActionPriority.Low, impact: ActionImpact.Low);
        pinned.IsNextBestAction = true;
        _repository.GetNextBestActionAsync(_dreamId, Arg.Any<CancellationToken>()).Returns(pinned);

        var result = await CreateHandler().Handle(new GetNextBestActionQuery(), CancellationToken.None);

        result!.Title.Should().Be("The one I chose");
        result.Rationale.Should().Be("You marked this as your next move.");
        // The computed fallback should never even be consulted once a pin exists.
        await _repository.DidNotReceive().GetForDreamAsync(Arg.Any<Guid>(), Arg.Any<ActionStatus?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Falls_back_to_a_computed_pick_with_a_rationale_when_nothing_is_pinned()
    {
        ArrangeSignedInUserWithDream();
        _repository.GetNextBestActionAsync(_dreamId, Arg.Any<CancellationToken>()).Returns((ActionItem?)null);
        var strong = MakeAction("Talk to five customers", priority: ActionPriority.High, impact: ActionImpact.High);
        var weak = MakeAction("Tidy notes", priority: ActionPriority.Low, impact: ActionImpact.Low);
        _repository.GetForDreamAsync(_dreamId, null, Arg.Any<CancellationToken>()).Returns([weak, strong]);

        var result = await CreateHandler().Handle(new GetNextBestActionQuery(), CancellationToken.None);

        result!.Title.Should().Be("Talk to five customers");
        result.Rationale.Should().NotBeNullOrWhiteSpace();
        result.Rationale.Should().NotBe("You marked this as your next move.");
    }

    [Fact]
    public async Task Returns_null_when_nothing_is_pinned_and_no_open_actions_exist()
    {
        ArrangeSignedInUserWithDream();
        _repository.GetNextBestActionAsync(_dreamId, Arg.Any<CancellationToken>()).Returns((ActionItem?)null);
        _repository.GetForDreamAsync(_dreamId, null, Arg.Any<CancellationToken>()).Returns([]);

        var result = await CreateHandler().Handle(new GetNextBestActionQuery(), CancellationToken.None);

        result.Should().BeNull();
    }
}

/// <summary>Minimal hand-written TimeProvider test double - avoids pulling in
/// Microsoft.Extensions.TimeProvider.Testing just for one fixed-clock test fixture.</summary>
internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
