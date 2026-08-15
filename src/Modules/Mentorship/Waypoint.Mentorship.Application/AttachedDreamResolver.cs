using Waypoint.Common;

namespace Waypoint.Mentorship.Application;

/// <summary>Shared helper so every handler resolves an optionally-attached Dream through
/// IDreamSummaryProvider the same way, projecting down to the lean AttachedDreamDto shape rather
/// than exposing the full DreamSummary to another Mentorship participant. Mirrors
/// Waypoint.Community.Application.AttachedDreamResolver and PersonResolver's single/batch
/// pattern.</summary>
internal static class AttachedDreamResolver
{
    public static async Task<AttachedDreamDto?> ResolveAsync(
        IDreamSummaryProvider provider, Guid? dreamId, CancellationToken cancellationToken)
    {
        if (dreamId is null)
        {
            return null;
        }

        var dream = await provider.GetByIdAsync(dreamId.Value, cancellationToken);
        return dream is null ? null : new AttachedDreamDto(dream.Title, dream.Statement);
    }

    public static async Task<IReadOnlyDictionary<Guid, AttachedDreamDto>> ResolveManyAsync(
        IDreamSummaryProvider provider, IReadOnlyList<Guid> dreamIds, CancellationToken cancellationToken)
    {
        var distinctIds = dreamIds.Distinct().ToList();
        if (distinctIds.Count == 0)
        {
            return new Dictionary<Guid, AttachedDreamDto>();
        }

        var dreams = await provider.GetByIdsAsync(distinctIds, cancellationToken);
        return dreams.ToDictionary(kv => kv.Key, kv => new AttachedDreamDto(kv.Value.Title, kv.Value.Statement));
    }
}
