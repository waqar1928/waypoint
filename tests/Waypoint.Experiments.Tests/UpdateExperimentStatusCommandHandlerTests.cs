using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Experiments.Application;
using Waypoint.Experiments.Application.UpdateExperimentStatus;
using Waypoint.Experiments.Domain;
using Xunit;

namespace Waypoint.Experiments.Tests;

public class UpdateExperimentStatusCommandHandlerTests
{
    private readonly IExperimentsRepository _repository = Substitute.For<IExperimentsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private UpdateExperimentStatusCommandHandler CreateHandler() => new(_repository, _dreamSummaryProvider, _currentUser);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    [Fact]
    public async Task Updates_the_status_of_an_own_experiment()
    {
        ArrangeSignedInUserWithDream();
        var experiment = Experiment.Create(_dreamId, _userId, "Idea", "Hypothesis", "Success criteria");
        _repository.GetByIdAsync(experiment.Id, Arg.Any<CancellationToken>()).Returns(experiment);
        _repository.GetResultsForExperimentsAsync(
            Arg.Is<IReadOnlyList<Guid>>(ids => ids.Contains(experiment.Id)), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await CreateHandler().Handle(
            new UpdateExperimentStatusCommand(experiment.Id, ExperimentStatus.Running), CancellationToken.None);

        experiment.Status.Should().Be(ExperimentStatus.Running);
        result.Status.Should().Be(ExperimentStatus.Running);
        await _repository.Received(1).SaveAsync(experiment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_the_experiment_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var othersExperiment = Experiment.Create(Guid.NewGuid(), _userId, "Not mine", "H", "S");
        _repository.GetByIdAsync(othersExperiment.Id, Arg.Any<CancellationToken>()).Returns(othersExperiment);

        var act = () => CreateHandler().Handle(
            new UpdateExperimentStatusCommand(othersExperiment.Id, ExperimentStatus.Running), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().SaveAsync(Arg.Any<Experiment>(), Arg.Any<CancellationToken>());
    }
}
