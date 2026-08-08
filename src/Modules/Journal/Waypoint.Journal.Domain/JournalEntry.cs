using Waypoint.Common;

namespace Waypoint.Journal.Domain;

public sealed class JournalEntry : Entity, ISoftDeletable
{
    public Guid UserId { get; init; }
    public Guid? DreamId { get; init; }
    public JournalEntryType EntryType { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }

    public static JournalEntry Create(Guid userId, Guid? dreamId, JournalEntryType entryType, string body) =>
        new()
        {
            UserId = userId,
            DreamId = dreamId,
            EntryType = entryType,
            Body = body,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
