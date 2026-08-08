using MediatR;
using Waypoint.Common;

namespace Waypoint.Journal.Application.GetMyJournalEntries;

public sealed record GetMyJournalEntriesQuery : IRequest<IReadOnlyList<JournalEntryDto>>;

public sealed class GetMyJournalEntriesQueryHandler(IJournalRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyJournalEntriesQuery, IReadOnlyList<JournalEntryDto>>
{
    private const int MaxRecentEntries = 20;

    public async Task<IReadOnlyList<JournalEntryDto>> Handle(
        GetMyJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var entries = await repository.GetRecentForUserAsync(userId, MaxRecentEntries, cancellationToken);

        return entries.Select(e => new JournalEntryDto(e.Id, e.EntryType, e.Body, e.CreatedAt)).ToList();
    }
}
