using Waypoint.Common;
using Waypoint.Experiments.Application;

namespace Waypoint.Experiments.Infrastructure;

/// <summary>Implements the cross-module IExperimentsSummaryProvider read contract — see Waypoint.Common/Auditing.cs.</summary>
public sealed class ExperimentsSummaryProvider(
    IExperimentsRepository repository, IDreamSummaryProvider dreamSummaryProvider)
    : IExperimentsSummaryProvider
{
    // Same reasoning as ActionsSummaryProvider's MaxItems - bounded for an AI prompt, not a page.
    private const int MaxItems = 4;

    public async Task<ExperimentsSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var experiments = await repository.GetForDreamAsync(dream.DreamId, cancellationToken);
        var recent = experiments.OrderByDescending(e => e.CreatedAt).Take(MaxItems).ToList();

        var experimentIds = recent.Select(e => e.Id).ToList();
        var results = await repository.GetResultsForExperimentsAsync(experimentIds, cancellationToken);
        var latestResultByExperiment = results
            .GroupBy(r => r.ExperimentId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedAt).First());

        var items = recent
            .Select(e =>
            {
                latestResultByExperiment.TryGetValue(e.Id, out var latestResult);
                return new ExperimentSummaryItem(
                    e.IdeaDescription,
                    e.Status.ToString(),
                    latestResult?.Outcome.ToString(),
                    latestResult?.Learning);
            })
            .ToList();

        return new ExperimentsSummary(items);
    }
}
