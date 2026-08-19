using MediatR;

namespace Waypoint.Common;

/// <summary>
/// Cross-module integration events. Per docs/03-domain-model.md "Module
/// communication rules", side effects that cross a module boundary (e.g.
/// Identity creating a user should cause Users to create a Profile) go
/// through MediatR notifications rather than one module's Application layer
/// referencing another's. Contracts live here, in the shared kernel, so
/// neither module depends on the other directly.
/// </summary>
public sealed record UserRegisteredIntegrationEvent(Guid UserId, string DisplayName, string Email) : INotification;

/// <summary>
/// Published after Identity has removed the account, so other modules can purge their own data
/// for that user (cascade deletion — see docs/PRODUCTION_READINESS_AUDIT.md's Data Protection
/// section). DreamId is resolved and snapshotted by DeleteAccountCommandHandler *before* the
/// account is deleted, then carried on the event itself, rather than each Dream-keyed module
/// (Goals/Actions/Experiments/BusinessIdeas — none of which store UserId directly, only DreamId)
/// re-resolving it live via IDreamSummaryProvider when handling this notification. That live
/// lookup would race against Dreams' own handler for this same event: MediatR's default publisher
/// invokes every INotificationHandler for one event in registration order, which is not something
/// this codebase pins or wants to depend on — if Dreams' handler happened to run first and delete
/// the Dream row, every other handler's live lookup would come back empty and their data would be
/// silently orphaned instead of deleted. Null if the user never completed onboarding (no Dream to
/// resolve).
/// </summary>
public sealed record UserDeletedIntegrationEvent(Guid UserId, Guid? DreamId) : INotification;

/// <summary>Published by Dreams when a user completes Discover/Define (their first Dream + Dream Statement is saved).</summary>
public sealed record OnboardingCompletedIntegrationEvent(Guid UserId) : INotification;

/// <summary>
/// Published whenever a user writes down something they learned somewhere other than the Journal
/// itself — today that's Experiments (RecordExperimentResultCommand's required Learning field) and
/// Actions (AddActionReflectionCommand's optional reflection on completion). Journal handles this
/// by writing a Lesson-type entry, which is what turns "learnings" from something trapped inside
/// one experiment's card into something that shows up in a single place. Not published when someone
/// writes a Journal entry directly — that's already a Journal entry, nothing needs to react to it.
/// </summary>
public sealed record LearningCapturedIntegrationEvent(Guid UserId, Guid DreamId, string Body) : INotification;
