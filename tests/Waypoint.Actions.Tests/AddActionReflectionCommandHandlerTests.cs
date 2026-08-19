using FluentAssertions;
using MediatR;
using NSubstitute;
using Waypoint.Actions.Application;
using Waypoint.Actions.Application.AddActionReflection;
using Waypoint.Actions.Domain;
using Waypoint.Common;
using Xunit;

namespace Waypoint.Actions.Tests;

public class AddActionReflectionCommandHandlerTests
{
    private readonly IActionsRepository _repository = Substitute.For<IActionsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private AddActionReflectionCommandHandler CreateHandler() =>
        new(_repository, _dreamSummaryProvider, _currentUser, _publisher);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    private ActionItem MakeCompletedAction() => new()
    {
        DreamId = _dreamId,
        Title = "Talked to five customers",
        Status = ActionStatus.Completed,
        CreatedBy = _userId,
        UpdatedBy = _userId,
    };

    [Fact]
    public async Task Publishes_a_combined_learning_when_both_fields_are_given()
    {
        ArrangeSignedInUserWithDream();
        var action = MakeCompletedAction();
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        await CreateHandler().Handle(
            new AddActionReflectionCommand(action.Id, "3 had the problem, 2 didn't", "Reporting matters more than automation"),
            CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<LearningCapturedIntegrationEvent>(e =>
                e.UserId == _userId && e.DreamId == _dreamId &&
                e.Body.Contains("3 had the problem, 2 didn't") &&
                e.Body.Contains("Reporting matters more than automation")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_just_the_learning_when_only_that_is_given()
    {
        ArrangeSignedInUserWithDream();
        var action = MakeCompletedAction();
        _repository.GetByIdAsync(action.Id, Arg.Any<CancellationToken>()).Returns(action);

        await CreateHandler().Handle(
            new AddActionReflectionCommand(action.Id, null, "People want a simpler tool"),
            CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<LearningCapturedIntegrationEvent>(e => e.Body == "People want a simpler tool"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_action_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var othersAction = new ActionItem
        {
            DreamId = Guid.NewGuid(), Title = "Not mine", Status = ActionStatus.Completed,
            CreatedBy = _userId, UpdatedBy = _userId,
        };
        _repository.GetByIdAsync(othersAction.Id, Arg.Any<CancellationToken>()).Returns(othersAction);

        var act = () => CreateHandler().Handle(
            new AddActionReflectionCommand(othersAction.Id, null, "Learning"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_when_the_action_is_not_completed()
    {
        ArrangeSignedInUserWithDream();
        var notDone = new ActionItem
        {
            DreamId = _dreamId, Title = "Still going", Status = ActionStatus.InProgress,
            CreatedBy = _userId, UpdatedBy = _userId,
        };
        _repository.GetByIdAsync(notDone.Id, Arg.Any<CancellationToken>()).Returns(notDone);

        var act = () => CreateHandler().Handle(
            new AddActionReflectionCommand(notDone.Id, null, "Learning"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}

public class AddActionReflectionCommandValidatorTests
{
    private readonly AddActionReflectionCommandValidator _validator = new();

    [Fact]
    public void Rejects_when_both_fields_are_blank()
    {
        var result = _validator.Validate(new AddActionReflectionCommand(Guid.NewGuid(), null, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accepts_when_only_learning_is_given()
    {
        var result = _validator.Validate(new AddActionReflectionCommand(Guid.NewGuid(), null, "Learned something"));

        result.IsValid.Should().BeTrue();
    }
}
