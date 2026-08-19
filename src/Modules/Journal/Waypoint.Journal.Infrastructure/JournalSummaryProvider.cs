using Waypoint.Common;
using Waypoint.Journal.Application;

namespace Waypoint.Journal.Infrastructure;

/// <summary>Implements the cross-module IJournalSummaryProvider read contract — see Waypoint.Common/Auditing.cs.</summary>
public sealed class JournalSummaryProvider(IJournalRepository repository) : IJournalSummaryProvider
{
    // Same bound as ActionsSummaryProvider/ExperimentsSummaryProvider — this feeds an AI prompt,
    // not a page.
    private const int MaxItems = 5;

    public async Task<JournalSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var lessons = await repository.GetRecentLessonsForUserAsync(userId, MaxItems, cancellationToken);
        if (lessons.Count == 0)
        {
            return null;
        }

        return new JournalSummary(lessons.Select(l => new LessonSummaryItem(l.Body, l.CreatedAt)).ToList());
    }
}
