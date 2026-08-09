using MediatR;
using Waypoint.Common;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Application.GetMyExperiments;

public sealed record GetMyExperimentsQuery : IRequest<IReadOnlyList<ExperimentDto>>;

public sealed class GetMyExperimentsQueryHandler(
    IExperimentsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyExperimentsQuery, IReadOnlyList<ExperimentDto>>
{
    public async Task<IReadOnlyList<ExperimentDto>> Handle(GetMyExperimentsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return [];
        }

        var experiments = await repository.GetForDreamAsync(dream.DreamId, cancellationToken);
        var experimentIds = experiments.Select(e => e.Id).ToList();
        var results = await repository.GetResultsForExperimentsAsync(experimentIds, cancellationToken);
        var resultsByExperiment = results
            .GroupBy(r => r.ExperimentId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ExperimentResult>)g.ToList());

        return experiments
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => ExperimentDto.From(e, resultsByExperiment.TryGetValue(e.Id, out var r) ? r : []))
            .ToList();
    }
}
