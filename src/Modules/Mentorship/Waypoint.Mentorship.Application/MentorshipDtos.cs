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

public sealed record HelpRequestDto(
    Guid Id,
    PersonDto Author,
    Guid? DreamId,
    HelpRequestCategory Category,
    string Title,
    string Body,
    HelpRequestStatus Status,
    int ResponseCount,
    bool IsMine,
    DateTimeOffset CreatedAt)
{
    public static HelpRequestDto From(HelpRequest request, PersonDto author, int responseCount, Guid currentUserId) => new(
        request.Id, author, request.DreamId, request.Category, request.Title, request.Body,
        request.Status, responseCount, request.UserId == currentUserId, request.CreatedAt);
}

public sealed record HelpRequestResponseDto(Guid Id, PersonDto Responder, string Body, bool IsMine, DateTimeOffset CreatedAt)
{
    public static HelpRequestResponseDto From(HelpRequestResponse response, PersonDto responder, Guid currentUserId) => new(
        response.Id, responder, response.Body, response.ResponderUserId == currentUserId, response.CreatedAt);
}
