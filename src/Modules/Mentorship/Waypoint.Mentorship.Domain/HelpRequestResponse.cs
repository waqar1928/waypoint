using Waypoint.Common;

namespace Waypoint.Mentorship.Domain;

/// <summary>
/// Fills a documented gap: docs/03-domain-model.md names "HelpRequest, responses" as what the
/// Mentorship module owns, but no responses table was ever schematized in
/// docs/04-database-design.md. Designed fresh for Phase 7 — a simple flat response, not threaded.
/// </summary>
public sealed class HelpRequestResponse : Entity
{
    public Guid HelpRequestId { get; init; }
    public Guid ResponderUserId { get; init; }
    public string Body { get; set; } = string.Empty;

    public static HelpRequestResponse Create(Guid helpRequestId, Guid responderUserId, string body) =>
        new()
        {
            HelpRequestId = helpRequestId,
            ResponderUserId = responderUserId,
            Body = body,
            CreatedBy = responderUserId,
            UpdatedBy = responderUserId,
        };
}
