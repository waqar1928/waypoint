using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Experiments.Application;
using Waypoint.Experiments.Application.RecordExperimentResult;
using Waypoint.Experiments.Domain;
using Xunit;

namespace Waypoint.Experiments.Tests;

public class RecordExperimentResultCommandHandlerTests
{
    private readonly IExperimentsRepository _repository = Substitute.For<IExperimentsRepository>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _dreamId = Guid.NewGuid();

    private RecordExperimentResultCommandHandler CreateHandler() =>
        new(_repository, _dreamSummaryProvider, _currentUser, _auditSink);

    private void ArrangeSignedInUserWithDream()
    {
        _currentUser.UserId.Returns(_userId);
        _dreamSummaryProvider
            .GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(_dreamId, _userId, "Title", "Statement", null, null, null, null, null, null, false));
    }

    [Fact]
    public async Task Recording_a_result_always_finalizes_status_to_completed()
    {
        ArrangeSignedInUserWithDream();
        var experiment = Experiment.Create(_dreamId, _userId, "Try a thing", "It will work", "5 signups");
        experiment.Status = ExperimentStatus.Running;
        _repository.GetByIdAsync(experiment.Id, Arg.Any<CancellationToken>()).Returns(experiment);
        _repository.GetResultsForExperimentsAsync(Arg.Any<IReadOnlyList<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var command = new RecordExperimentResultCommand(experiment.Id, ExperimentOutcome.Validated, "Got 5 signups", "People want this");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.Status.Should().Be(ExperimentStatus.Completed);
        await _repository.Received(1).AddResultAsync(
            Arg.Is<ExperimentResult>(r => r.Outcome == ExperimentOutcome.Validated && r.Learning == "People want this"),
            Arg.Any<CancellationToken>());
        await _auditSink.Received(1).RecordAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_when_experiment_belongs_to_a_different_dream()
    {
        ArrangeSignedInUserWithDream();
        var othersExperiment = Experiment.Create(Guid.NewGuid(), _userId, "Not mine", "N/A", "N/A");
        _repository.GetByIdAsync(othersExperiment.Id, Arg.Any<CancellationToken>()).Returns(othersExperiment);

        var command = new RecordExperimentResultCommand(othersExperiment.Id, ExperimentOutcome.Validated, null, "Learning");
        var act = () => CreateHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_when_experiment_does_not_exist()
    {
        ArrangeSignedInUserWithDream();
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Experiment?)null);

        var command = new RecordExperimentResultCommand(Guid.NewGuid(), ExperimentOutcome.Invalidated, null, "Learning");
        var act = () => CreateHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
