using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.DismissReport;

public sealed record DismissReportCommand(Guid ReportId) : IRequest;

public sealed class DismissReportCommandHandler(ICommunityRepository repository)
    : IRequestHandler<DismissReportCommand>
{
    public async Task Handle(DismissReportCommand request, CancellationToken cancellationToken)
    {
        var report = await repository.GetReportByIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException("Report not found.");

        report.Status = ReportStatus.Dismissed;
        await repository.SaveReportAsync(report, cancellationToken);
    }
}
