using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Goals.Application;
using Waypoint.Goals.Application.Milestones;
using Waypoint.Goals.Domain;
using Xunit;

namespace Waypoint.Goals.Tests;

/// <summary>
/// Regression coverage for the Phase 10 xmin-concurrency fix: this handler used to load the
/// entire milestone list and filter to one item in memory (which broke once the repository read
/// switched to AsNoTracking(), since the xmin shadow value never got captured); it now goes
/// through a dedicated tracked single-row lookup instead. These tests pin the current, correct
/// behavior — ownership check plus a one-way AchievedAt transition.
/// </summary>
public class MarkMilestoneAchievedCommandHandlerTests
{
    private readonly IGoalsRepository _repository = Substitute.For<IGoalsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private MarkMilestoneAchievedCommandHandler CreateHandler() => new(_repository, _dreamSummaryProvider, _currentUser);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    [Fact]
    public async Task Marks_an_own_milestone_achieved_via_the_tracked_single_row_lookup()
    {
        ArrangeSignedInUserWithDream();
        var milestone = Milestone.Create(_dreamId, _userId, "First paying customer", isCustom: true);
        _repository.GetMilestoneByIdAsync(milestone.Id, Arg.Any<CancellationToken>()).Returns(milestone);

        var result = await CreateHandler().Handle(new MarkMilestoneAchievedCommand(milestone.Id), CancellationToken.None);

        milestone.AchievedAt.Should().NotBeNull();
        result.AchievedAt.Should().NotBeNull();
        await _repository.Received(1).SaveMilestoneAsync(milestone, Arg.Any<CancellationToken>());
        // The old, buggy implementation went through GetMilestonesForDreamAsync (the full list) —
        // assert it's never called from this handler anymore.
        await _repository.DidNotReceive().GetMilestonesForDreamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_milestone_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var othersMilestone = Milestone.Create(Guid.NewGuid(), _userId, "Not mine", isCustom: true);
        _repository.GetMilestoneByIdAsync(othersMilestone.Id, Arg.Any<CancellationToken>()).Returns(othersMilestone);

        var act = () => CreateHandler().Handle(new MarkMilestoneAchievedCommand(othersMilestone.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().SaveMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_milestone_does_not_exist()
    {
        ArrangeSignedInUserWithDream();
        _repository.GetMilestoneByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Milestone?)null);

        var act = () => CreateHandler().Handle(new MarkMilestoneAchievedCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
