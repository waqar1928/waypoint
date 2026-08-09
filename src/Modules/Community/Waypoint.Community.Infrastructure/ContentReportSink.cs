using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Infrastructure;

/// <summary>Implements the shared IContentReportSink port — see Waypoint.Common/Auditing.cs.</summary>
public sealed class ContentReportSink(CommunityDbContext db) : IContentReportSink
{
    public async Task RecordAsync(ContentReport report, CancellationToken cancellationToken)
    {
        var record = ContentReportRecord.Create(
            report.EntityType, report.EntityId, report.ReporterUserId, report.Reason, report.Details);
        db.ContentReports.Add(record);
        await db.SaveChangesAsync(cancellationToken);
    }
}
