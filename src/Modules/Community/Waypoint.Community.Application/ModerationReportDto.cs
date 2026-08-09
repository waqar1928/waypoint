using Waypoint.Community.Domain;

namespace Waypoint.Community.Application;

public sealed record ModerationReportDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Reason,
    string? Details,
    ReportStatus Status,
    Guid ReporterUserId,
    string? ContentPreview,
    DateTimeOffset CreatedAt);
