using FluentAssertions;
using NSubstitute;
using Waypoint.Actions.Application;
using Waypoint.Actions.Application.UpdateActionStatus;
using Waypoint.Actions.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.Actions.Tests;

public class UpdateActionStatusCommandHandlerTests
{
    private readonly IActionsRepository _repository = Substitute.For<IActionsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly IProductAnalyticsSink _analyticsSink = Substitute.For<IProductAnalyticsSink>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private UpdateActionStatusCommandHandler CreateHandler() =>
        new(_repository, _dreamSummaryProvider, _currentUser, _auditSink, _analyticsSink);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    private ActionItem BuildAction(bool isNextBest = false)
    {
        var action = ActionItem.Create(_dreamId, _userId, "Do the thing", null,
            ActionPriority.Medium, ActionDifficulty.Medium, null, ActionImpact.Medium, null, null, null);
        action.IsNextBestAction = isNextBest;
        return action;
    }

    [Fact]
    public async Task Completing_an_action_sets_completed_at_clears_next_best_flag_and_records_an_audit_entry()
    {
        ArrangeSignedInUserWithDream();
        var action = BuildAction(isNextBest: true);
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        var result = await CreateHandler().Handle(
            new UpdateActionStatusCommand(action.Id, ActionStatus.Completed), CancellationToken.None);

        result.Status.Should().Be(ActionStatus.Completed);
        action.CompletedAt.Should().NotBeNull();
        action.IsNextBestAction.Should().BeFalse();
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "Completed" && e.EntityId == action.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cancelling_an_action_clears_next_best_flag_but_records_no_audit_entry()
    {
        ArrangeSignedInUserWithDream();
        var action = BuildAction(isNextBest: true);
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        await CreateHandler().Handle(new UpdateActionStatusCommand(action.Id, ActionStatus.Cancelled), CancellationToken.None);

        action.IsNextBestAction.Should().BeFalse();
        action.CompletedAt.Should().BeNull();
        await _auditSink.DidNotReceive().RecordAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reopening_a_completed_action_clears_completed_at()
    {
        ArrangeSignedInUserWithDream();
        var action = BuildAction();
        action.Status = ActionStatus.Completed;
        action.CompletedAt = DateTimeOffset.UtcNow;
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        await CreateHandler().Handle(new UpdateActionStatusCommand(action.Id, ActionStatus.InProgress), CancellationToken.None);

        action.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Throws_when_the_action_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var othersAction = ActionItem.Create(Guid.NewGuid(), _userId, "Not mine", null,
            ActionPriority.Medium, ActionDifficulty.Medium, null, ActionImpact.Medium, null, null, null);
        _repository.GetByIdAsync(othersAction.Id, Arg.Any<CancellationToken>()).Returns(othersAction);

        var act = () => CreateHandler().Handle(
            new UpdateActionStatusCommand(othersAction.Id, ActionStatus.Completed), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
