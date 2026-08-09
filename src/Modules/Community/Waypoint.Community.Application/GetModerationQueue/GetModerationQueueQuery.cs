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
        var result = new List<ModerationReportDto>();

        foreach (var report in reports)
        {
            string? preview = report.EntityType switch
            {
                ReportableEntityTypes.Post => (await repository.GetPostByIdAsync(report.EntityId, cancellationToken))?.Body,
                ReportableEntityTypes.Comment => (await repository.GetCommentByIdAsync(report.EntityId, cancellationToken))?.Body,
                _ => null,
            };

            result.Add(new ModerationReportDto(
                report.Id, report.EntityType, report.EntityId, report.Reason, report.Details,
                report.Status, report.ReporterUserId, preview, report.CreatedAt));
        }

        return result;
    }
}
