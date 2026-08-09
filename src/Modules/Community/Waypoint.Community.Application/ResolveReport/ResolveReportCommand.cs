using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.ResolveReport;

/// <summary>For entity types Community can't act on directly (e.g. help_request) — marks the
/// report reviewed without removing anything, since that decision belongs to Mentorship.</summary>
public sealed record ResolveReportCommand(Guid ReportId) : IRequest;

public sealed class ResolveReportCommandHandler(ICommunityRepository repository)
    : IRequestHandler<ResolveReportCommand>
{
    public async Task Handle(ResolveReportCommand request, CancellationToken cancellationToken)
    {
        var report = await repository.GetReportByIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException("Report not found.");

        report.Status = ReportStatus.Resolved;
        await repository.SaveReportAsync(report, cancellationToken);
    }
}
