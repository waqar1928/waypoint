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

/// <summary>Published after Identity has removed the account, so other modules can purge their own data for that user.</summary>
public sealed record UserDeletedIntegrationEvent(Guid UserId) : INotification;
