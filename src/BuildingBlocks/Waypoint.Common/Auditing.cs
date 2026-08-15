namespace Waypoint.Common;

public sealed record AuditEntry(
    string EntityType,
    Guid EntityId,
    string Action,
    Guid? ActorUserId,
    string? PayloadRedacted,
    DateTimeOffset OccurredAt);

/// <summary>
/// Cross-cutting audit port (see docs/03-domain-model.md "Cross-cutting concerns").
/// Every module writes through this interface; only Waypoint.Audit.Infrastructure
/// implements it, so no module needs a reference to the Audit module's tables.
/// </summary>
public interface IAuditSink
{
    Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken);
}

/// <summary>Identifies who is making the current request, without any module depending on ASP.NET Core directly.</summary>
public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsInRole(string role);
}

/// <summary>
/// Each module's Infrastructure layer implements this over its own
/// DbContext. The host resolves every registered migrator at startup
/// (see docs/04-database-design.md "Migration strategy") so one module can
/// ship a migration without coordinating with another module's history.
/// </summary>
public interface IStartupMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the Dreams module — the concrete
/// example named in docs/03-domain-model.md "Module communication rules"
/// (rule 2). Goals and Actions depend on this interface only, never on
/// Dreams' Application layer or DbContext directly.
/// </summary>
public sealed record DreamSummary(
    Guid DreamId,
    Guid UserId,
    string Title,
    string Statement,
    string? Purpose,
    string? WhoItHelps,
    string? Problem,
    string? Outcome,
    string? Motivation,
    string? Impact,
    bool IsBusinessShaped);

public interface IDreamSummaryProvider
{
    Task<DreamSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a Dream by its own Id rather than by owner - added for Community/Mentorship's
    /// optional "attach my Dream" feature, where a post/help-request author's OWN Dream is
    /// resolved and stored server-side at creation time (see CreatePostCommand/
    /// CreateHelpRequestCommand — deliberately a bool AttachDream flag, never a client-supplied
    /// DreamId, so there's no way to attach a Dream that isn't yours), then read back here by
    /// other users viewing the feed. Never trust a caller-supplied dreamId for anything
    /// authorization-sensitive; this method only resolves already-validated, already-stored Ids.
    /// </summary>
    Task<DreamSummary?> GetByIdAsync(Guid dreamId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, DreamSummary>> GetByIdsAsync(
        IReadOnlyList<Guid> dreamIds, CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the BusinessIdeas module (Phase 5) — same pattern as
/// IDreamSummaryProvider. The AI module (Phase 6) depends on this to seed "Challenge my idea"
/// conversations without ever touching BusinessIdeas' Application layer or DbContext directly
/// (see docs/03-domain-model.md "Module communication rules").
/// </summary>
public sealed record BusinessIdeaSummary(
    Guid BusinessIdeaId,
    Guid DreamId,
    string? Problem,
    string? Customer,
    string? ValueProposition,
    string? Solution,
    string? BusinessModel,
    string? Market,
    string? Competitors,
    string? Pricing,
    string? Marketing,
    string? Sales,
    string? Operations,
    string? Technology,
    string? FinancialAssumptions,
    string? Risks);

public interface IBusinessIdeaSummaryProvider
{
    Task<BusinessIdeaSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the Actions module. Lets AI Coach optionally include a
/// snapshot of what the user is actually working on in a conversation's opening context, not just
/// their Dream Statement (see StartConversationCommand's IncludeProgressContext flag — opt-in per
/// conversation, off by default). Deliberately a small, bounded summary rather than the full
/// action list, since this gets interpolated into an AI prompt, not rendered as a page — the
/// summary's job is to describe what's going on, not enumerate everything.
/// </summary>
public sealed record ActionSummaryItem(string Title, string Status, bool IsNextBestAction);

public sealed record ActionsSummary(IReadOnlyList<ActionSummaryItem> RecentActions);

public interface IActionsSummaryProvider
{
    Task<ActionsSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the Experiments module — same reasoning and same opt-in
/// flag as IActionsSummaryProvider.
/// </summary>
public sealed record ExperimentSummaryItem(string IdeaDescription, string Status, string? LatestOutcome, string? LatestLearning);

public sealed record ExperimentsSummary(IReadOnlyList<ExperimentSummaryItem> RecentExperiments);

public interface IExperimentsSummaryProvider
{
    Task<ExperimentsSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}

/// <summary>
/// Cross-cutting content-report port (Phase 7) — same pattern as IAuditSink. Community owns the
/// actual content_reports table and implements this; Mentorship (help requests) files reports
/// through the interface only, so it never needs a direct reference to Community's tables. Phase
/// 8's admin moderation queue reads against the same table this writes to.
/// </summary>
public sealed record ContentReport(
    string EntityType,
    Guid EntityId,
    Guid ReporterUserId,
    string Reason,
    string? Details,
    DateTimeOffset OccurredAt);

public interface IContentReportSink
{
    Task RecordAsync(ContentReport report, CancellationToken cancellationToken);
}

/// <summary>
/// Cross-module read contract owned by the Users module — lets Community and Mentorship (Phase 7)
/// show a display name/avatar next to a post, comment, or mentor profile without either module
/// depending on Users' Application layer or DbContext directly. The batch method exists so a
/// feed of N posts by M distinct authors costs one query, not N.
/// </summary>
public sealed record ProfileSummary(Guid UserId, string DisplayName, string? AvatarUrl);

public interface IProfileSummaryProvider
{
    Task<ProfileSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, ProfileSummary>> GetForUsersAsync(IReadOnlyList<Guid> userIds, CancellationToken cancellationToken);
}
