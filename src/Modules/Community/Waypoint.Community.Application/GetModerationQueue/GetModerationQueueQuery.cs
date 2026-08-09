using MediatR;
using Waypoint.Community.Application.ReportContent;

namespace Waypoint.Community.Application.GetModerationQueue;

public sealed record GetModerationQueueQuery : IRequest<IReadOnlyList<ModerationReportDto>>;

/// <summary>
/// Community owns content_reports (see ContentReportRecord) so it can resolve a real preview for
/// its own entity types (post/comment) directly. For other entity types written through the
/// shared IContentReportSink (currently just help_request, from Mentorship), there's no preview —
/// only the type/id and the report itself, since Community has no access to Mentorship's tables
/// (module boundary). The admin UI shows a generic label for those.
/// </summary>
public sealed class GetModerationQueueQueryHandler(ICommunityRepository repository)
    : IRequestHandler<GetModerationQueueQuery, IReadOnlyList<ModerationReportDto>>
{
    public async Task<IReadOnlyList<ModerationReportDto>> Handle(GetModerationQueueQuery request, CancellationToken cancellationToken)
    {
        var reports = await repository.GetOpenReportsAsync(cancellationToken);

        // Batched by entity type instead of one lookup per report — a queue of 50 open reports
        // issues 3 queries total (reports + posts-by-id + comments-by-id) instead of up to 51.
        var postIds = reports.Where(r => r.EntityType == ReportableEntityTypes.Post).Select(r => r.EntityId).ToList();
        var commentIds = reports.Where(r => r.EntityType == ReportableEntityTypes.Comment).Select(r => r.EntityId).ToList();

        var posts = await repository.GetPostsByIdsAsync(postIds, cancellationToken);
        var comments = await repository.GetCommentsByIdsAsync(commentIds, cancellationToken);

        return reports
            .Select(report =>
            {
                string? preview = report.EntityType switch
                {
                    ReportableEntityTypes.Post => posts.TryGetValue(report.EntityId, out var post) ? post.Body : null,
                    ReportableEntityTypes.Comment => comments.TryGetValue(report.EntityId, out var comment) ? comment.Body : null,
                    _ => null,
                };

                return new ModerationReportDto(
                    report.Id, report.EntityType, report.EntityId, report.Reason, report.Details,
                    report.Status, report.ReporterUserId, preview, report.CreatedAt);
            })
            .ToList();
    }
}
