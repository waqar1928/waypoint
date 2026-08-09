using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Goals.Application;
using Waypoint.Goals.Application.UpdateGoal;
using Waypoint.Goals.Domain;
using Xunit;

namespace Waypoint.Goals.Tests;

public class UpdateGoalCommandHandlerTests
{
    private readonly IGoalsRepository _repository = Substitute.For<IGoalsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private UpdateGoalCommandHandler CreateHandler() => new(_repository, _dreamSummaryProvider, _currentUser);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    [Fact]
    public async Task Updates_the_statement_and_target_date_of_a_goal_on_the_users_own_dream()
    {
        ArrangeSignedInUserWithDream();
        var goal = Goal.Create(_dreamId, _userId, GoalHorizon.OneYear, "Old statement", null);
        _repository.GetGoalByIdAsync(goal.Id, Arg.Any<CancellationToken>()).Returns(goal);
        var newTargetDate = new DateOnly(2027, 1, 1);

        var result = await CreateHandler().Handle(
            new UpdateGoalCommand(goal.Id, "New statement", newTargetDate), CancellationToken.None);

        goal.Statement.Should().Be("New statement");
        goal.TargetDate.Should().Be(newTargetDate);
        result.Statement.Should().Be("New statement");
        await _repository.Received(1).SaveGoalAsync(goal, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_goal_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var othersGoal = Goal.Create(Guid.NewGuid(), _userId, GoalHorizon.FiveYear, "Not mine", null);
        _repository.GetGoalByIdAsync(othersGoal.Id, Arg.Any<CancellationToken>()).Returns(othersGoal);

        var act = () => CreateHandler().Handle(
            new UpdateGoalCommand(othersGoal.Id, "Hijacked", null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().SaveGoalAsync(Arg.Any<Goal>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_goal_does_not_exist()
    {
        ArrangeSignedInUserWithDream();
        _repository.GetGoalByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Goal?)null);

        var act = () => CreateHandler().Handle(
            new UpdateGoalCommand(Guid.NewGuid(), "Statement", null), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
