using Waypoint.Common;

namespace Waypoint.Community.Domain;

/// <summary>
/// The storage side of the shared IContentReportSink port (see Waypoint.Common/Auditing.cs).
/// Community is the natural owner since most reportable content lives here, but Mentorship
/// (help requests) writes into the same table through the interface. Phase 8's admin moderation
/// queue reads this table directly; nothing reviews it yet.
/// </summary>
public sealed class ContentReportRecord : Entity
{
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public Guid ReporterUserId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Details { get; init; }
    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public static ContentReportRecord Create(string entityType, Guid entityId, Guid reporterUserId, string reason, string? details) =>
        new()
        {
            EntityType = entityType,
            EntityId = entityId,
            ReporterUserId = reporterUserId,
            Reason = reason,
            Details = details,
            CreatedBy = reporterUserId,
            UpdatedBy = reporterUserId,
        };
}
