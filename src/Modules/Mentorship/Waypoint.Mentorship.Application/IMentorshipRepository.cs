using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application;

public interface IMentorshipRepository
{
    Task<MentorProfile?> GetMentorProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<MentorProfile?> GetMentorProfileByIdAsync(Guid mentorProfileId, CancellationToken cancellationToken);
    /// <summary>Capped at <paramref name="take"/> — see the in-memory expertise-filter note on the
    /// implementation for why this can't push the filter into SQL yet.</summary>
    Task<IReadOnlyList<MentorProfile>> GetMentorDirectoryAsync(string? expertiseFilter, int take, CancellationToken cancellationToken);
    Task AddMentorProfileAsync(MentorProfile profile, CancellationToken cancellationToken);
    Task SaveMentorProfileAsync(MentorProfile profile, CancellationToken cancellationToken);

    Task<HelpRequest?> GetHelpRequestByIdAsync(Guid helpRequestId, CancellationToken cancellationToken);

    /// <summary>Most-recent-first, capped at <paramref name="take"/> — a global, cross-user board
    /// with no natural per-user bound, so it needs a safety cap as the user base grows.</summary>
    Task<IReadOnlyList<HelpRequest>> GetHelpRequestsAsync(
        HelpRequestCategory? categoryFilter, HelpRequestStatus? statusFilter, int take, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpRequest>> GetHelpRequestsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task AddHelpRequestAsync(HelpRequest request, CancellationToken cancellationToken);
    Task SaveHelpRequestAsync(HelpRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> GetResponseCountsAsync(IReadOnlyList<Guid> helpRequestIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<HelpRequestResponse>> GetResponsesForHelpRequestAsync(Guid helpRequestId, CancellationToken cancellationToken);
    Task AddResponseAsync(HelpRequestResponse response, CancellationToken cancellationToken);

    /// <summary>Real hard delete of everything this user owns — backs account deletion (see
    /// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section). Covers: this user's mentor
    /// profile (if any), every response on this user's own help requests (regardless of
    /// responder, since the request is gone), this user's own responses on other people's
    /// requests, and this user's own help requests. No DB-level cascade exists between any of
    /// these tables, so each relationship is handled explicitly.</summary>
    Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
