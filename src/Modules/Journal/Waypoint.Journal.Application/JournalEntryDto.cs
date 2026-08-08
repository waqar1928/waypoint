using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Application;

public sealed record JournalEntryDto(Guid Id, JournalEntryType EntryType, string Body, DateTimeOffset CreatedAt);
