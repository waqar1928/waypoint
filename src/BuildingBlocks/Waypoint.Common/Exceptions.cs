namespace Waypoint.Common;

/// <summary>Maps to HTTP 404 in the global exception handler.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Maps to HTTP 409 in the global exception handler.</summary>
public sealed class ConflictException(string message) : Exception(message);

/// <summary>Maps to HTTP 401 in the global exception handler.</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);

/// <summary>Maps to HTTP 423 (locked) in the global exception handler.</summary>
public sealed class AccountLockedException(string message) : Exception(message);

/// <summary>
/// Maps to HTTP 403 in the global exception handler. Thrown on a login attempt for an account
/// that exists and has the right password but hasn't confirmed its email yet — deliberately a
/// distinct exception type from AuthenticationFailedException so the frontend can offer a
/// "resend verification email" action instead of a plain "wrong password" message.
/// </summary>
public sealed class EmailNotConfirmedException(string message) : Exception(message);

/// <summary>
/// Maps to HTTP 503 in the global exception handler. Thrown when an AI provider call can't be
/// completed — missing configuration, a transient failure, or a moderation-blocked response —
/// so the frontend can show the user a clear, non-generic message instead of a bare 500.
/// </summary>
public sealed class AiServiceUnavailableException(string message) : Exception(message);
