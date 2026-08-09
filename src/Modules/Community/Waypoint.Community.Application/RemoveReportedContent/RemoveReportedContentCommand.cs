using MediatR;
using Waypoint.Common;
using Waypoint.Community.Application.ReportContent;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.RemoveReportedContent;

/// <summary>Only valid for post/comment reports — the only entity types Community can soft-delete
/// directly. Help request reports must go through ResolveReportCommand instead.</summary>
public sealed record RemoveReportedContentCommand(Guid ReportId) : IRequest;

public sealed class RemoveReportedContentCommandHandler(ICommunityRepository repository)
    : IRequestHandler<RemoveReportedContentCommand>
{
    public async Task Handle(RemoveReportedContentCommand request, CancellationToken cancellationToken)
    {
        var report = await repository.GetReportByIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException("Report not found.");

        switch (report.EntityType)
        {
            case ReportableEntityTypes.Post:
                var post = await repository.GetPostByIdAsync(report.EntityId, cancellationToken);
                if (post is not null)
                {
                    post.DeletedAt = DateTimeOffset.UtcNow;
                    await repository.SavePostAsync(post, cancellationToken);
                }
                break;

            case ReportableEntityTypes.Comment:
                var comment = await repository.GetCommentByIdAsync(report.EntityId, cancellationToken);
                if (comment is not null)
                {
                    comment.DeletedAt = DateTimeOffset.UtcNow;
                    await repository.SaveCommentAsync(comment, cancellationToken);
                }
                break;

            default:
                throw new ConflictException(
                    $"Content of type '{report.EntityType}' can't be removed from here — use Resolve instead.");
        }

        report.Status = ReportStatus.ContentRemoved;
        await repository.SaveReportAsync(report, cancellationToken);
    }
}
