using FluentAssertions;
using NSubstitute;
using Waypoint.Actions.Application;
using Waypoint.Actions.Application.SetNextBestAction;
using Waypoint.Actions.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.Actions.Tests;

public class SetNextBestActionCommandHandlerTests
{
    private readonly IActionsRepository _repository = Substitute.For<IActionsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private SetNextBestActionCommandHandler CreateHandler() => new(_repository, _dreamSummaryProvider, _currentUser);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    [Fact]
    public async Task Clears_every_other_next_best_action_before_setting_this_one()
    {
        ArrangeSignedInUserWithDream();
        var action = ActionItem.Create(_dreamId, _userId, "Do the thing", null,
            ActionPriority.Medium, ActionDifficulty.Medium, null, ActionImpact.Medium, null, null, null);
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        var result = await CreateHandler().Handle(new SetNextBestActionCommand(action.Id), CancellationToken.None);

        result.IsNextBestAction.Should().BeTrue();
        await _repository.Received(1).ClearNextBestActionForDreamAsync(_dreamId, action.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_action_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var otherDreamsAction = ActionItem.Create(Guid.NewGuid(), _userId, "Not mine", null,
            ActionPriority.Medium, ActionDifficulty.Medium, null, ActionImpact.Medium, null, null, null);
        _repository.GetByIdAsync(otherDreamsAction.Id, Arg.Any<CancellationToken>()).Returns(otherDreamsAction);

        var act = () => CreateHandler().Handle(new SetNextBestActionCommand(otherDreamsAction.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_when_action_is_already_completed()
    {
        ArrangeSignedInUserWithDream();
        var action = ActionItem.Create(_dreamId, _userId, "Done already", null,
            ActionPriority.Medium, ActionDifficulty.Medium, null, ActionImpact.Medium, null, null, null);
        action.Status = ActionStatus.Completed;
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        var act = () => CreateHandler().Handle(new SetNextBestActionCommand(action.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
