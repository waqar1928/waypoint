namespace Waypoint.Community.Domain;

/// <summary>Phase 8 moderation queue outcomes. ContentRemoved only applies to post/comment
/// reports (the only entity types Community can soft-delete directly); help_request reports can
/// only reach Dismissed or Resolved.</summary>
public enum ReportStatus { Open, Dismissed, ContentRemoved, Resolved }
