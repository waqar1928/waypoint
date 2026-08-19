using MediatR;
using Waypoint.Common;

namespace Waypoint.Journal.Application.GetRecentLearnings;

/// <summary>
/// The user-facing counterpart to IJournalSummaryProvider (which feeds the Coach's prompt): this
/// is what backs the "Learnings" feed shown on the Dream Overview and dashboard, so a learning
/// captured from an Experiment result or an Action reflection - see LearningCapturedIntegrationEvent
/// - actually shows up somewhere the user sees it again, not just inside the Coach's context.
/// </summary>
public sealed record GetRecentLearningsQuery : IRequest<IReadOnlyList<JournalEntryDto>>;

public sealed class GetRecentLearningsQueryHandler(IJournalRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetRecentLearningsQuery, IReadOnlyList<JournalEntryDto>>
{
    private const int MaxItems = 10;

    public async Task<IReadOnlyList<JournalEntryDto>> Handle(
        GetRecentLearningsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var lessons = await repository.GetRecentLessonsForUserAsync(userId, MaxItems, cancellationToken);

        return lessons.Select(e => new JournalEntryDto(e.Id, e.EntryType, e.Body, e.CreatedAt)).ToList();
    }
}
