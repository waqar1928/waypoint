using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Application;

public sealed record PersonDto(Guid UserId, string DisplayName, string? AvatarUrl);

public sealed record MentorProfileDto(
    Guid Id,
    PersonDto Mentor,
    IReadOnlyList<string> Expertise,
    int? YearsExperience,
    string? Availability,
    VerificationStatus VerificationStatus)
{
    public static MentorProfileDto From(MentorProfile profile, PersonDto mentor) => new(
        profile.Id, mentor, profile.Expertise, profile.YearsExperience, profile.Availability, profile.VerificationStatus);
}

/// <summary>
/// Deliberately lean - title and statement only. Same reasoning as Community's identical
/// AttachedDreamDto: attaching a Dream to a help request is opt-in (see CreateHelpRequestCommand's
/// AttachDream flag), but a mentor reading this doesn't need the same depth of detail Drevia
/// Coach gets - this is what's shown to another person, not what's stored privately. Kept as its
/// own type per-module (matching how PersonDto/AuthorDto are each module-local too) rather than a
/// new shared type in Waypoint.Common, since the only actual cross-module contract needed here is
/// IDreamSummaryProvider itself.
/// </summary>
public sealed record AttachedDreamDto(string Title, string Statement);

public sealed record HelpRequestDto(
    Guid Id,
    PersonDto Author,
    Guid? DreamId,
    AttachedDreamDto? AttachedDream,
    HelpRequestCategory Category,
    string Title,
    string Body,
    HelpRequestStatus Status,
    int ResponseCount,
    bool IsMine,
    DateTimeOffset CreatedAt)
{
    public static HelpRequestDto From(
        HelpRequest request, PersonDto author, AttachedDreamDto? attachedDream, int responseCount, Guid currentUserId) => new(
        request.Id, author, request.DreamId, attachedDream, request.Category, request.Title, request.Body,
        request.Status, responseCount, request.UserId == currentUserId, request.CreatedAt);
}

public sealed record HelpRequestResponseDto(Guid Id, PersonDto Responder, string Body, bool IsMine, DateTimeOffset CreatedAt)
{
    public static HelpRequestResponseDto From(HelpRequestResponse response, PersonDto responder, Guid currentUserId) => new(
        response.Id, responder, response.Body, response.ResponderUserId == currentUserId, response.CreatedAt);
}
