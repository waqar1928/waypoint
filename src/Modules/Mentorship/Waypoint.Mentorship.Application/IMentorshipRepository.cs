using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application;

public interface IMentorshipRepository
{
    Task<MentorProfile?> GetMentorProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<MentorProfile?> GetMentorProfileByIdAsync(Guid mentorProfileId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MentorProfile>> GetMentorDirectoryAsync(string? expertiseFilter, CancellationToken cancellationToken);
    Task AddMentorProfileAsync(MentorProfile profile, CancellationToken cancellationToken);
    Task SaveMentorProfileAsync(MentorProfile profile, CancellationToken cancellationToken);

    Task<HelpRequest?> GetHelpRequestByIdAsync(Guid helpRequestId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpRequest>> GetHelpRequestsAsync(
        HelpRequestCategory? categoryFilter, HelpRequestStatus? statusFilter, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpRequest>> GetHelpRequestsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task AddHelpRequestAsync(HelpRequest request, CancellationToken cancellationToken);
    Task SaveHelpRequestAsync(HelpRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> GetResponseCountsAsync(IReadOnlyList<Guid> helpRequestIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<HelpRequestResponse>> GetResponsesForHelpRequestAsync(Guid helpRequestId, CancellationToken cancellationToken);
    Task AddResponseAsync(HelpRequestResponse response, CancellationToken cancellationToken);
}
