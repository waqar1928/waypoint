using Microsoft.AspNetCore.Identity;

namespace Waypoint.Identity.Infrastructure;

/// <summary>
/// The Identity module's user record. Deliberately not exposed outside this
/// project — Application-layer code only ever sees the port types in
/// Waypoint.Identity.Application (UserLookup, SignInResult, etc.).
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>;
