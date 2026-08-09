using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Experiments.Application;
using Waypoint.Experiments.Application.CreateExperiment;
using Waypoint.Experiments.Domain;
using Xunit;

namespace Waypoint.Experiments.Tests;

public class CreateExperimentCommandHandlerTests
{
    private readonly IExperimentsRepository _repository = Substitute.For<IExperimentsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private CreateExperimentCommandHandler CreateHandler() => new(_repository, _dreamSummaryProvider, _currentUser);

    [Fact]
    public async Task Creates_a_planned_experiment_scoped_to_the_users_dream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));

        var command = new CreateExperimentCommand("Post in Facebook groups", "5 replies in a week", "5+ replies");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.Status.Should().Be(ExperimentStatus.Planned);
        result.Results.Should().BeEmpty();
        await _repository.Received(1).AddAsync(
            Arg.Is<Experiment>(e => e.DreamId == _dreamId && e.Status == ExperimentStatus.Planned),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_user_has_no_dream_yet()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((DreamSummary?)null);

        var command = new CreateExperimentCommand("Idea", "Hypothesis", "Criteria");
        var act = () => CreateHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
