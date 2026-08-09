using Waypoint.Common;

namespace Waypoint.Mentorship.Domain;

/// <summary>Not soft-deletable by design — docs/03-domain-model.md's soft-delete enumeration
/// (Dream, Goal, Action, JournalEntry, CommunityPost) deliberately excludes HelpRequest.</summary>
public sealed class HelpRequest : Entity
{
    public Guid UserId { get; init; }
    public Guid? DreamId { get; init; }
    public HelpRequestCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public HelpRequestStatus Status { get; set; } = HelpRequestStatus.Open;

    public static HelpRequest Create(
        Guid userId, Guid? dreamId, HelpRequestCategory category, string title, string body) =>
        new()
        {
            UserId = userId,
            DreamId = dreamId,
            Category = category,
            Title = title,
            Body = body,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
