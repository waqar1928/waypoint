namespace Waypoint.Common;

/// <summary>Maps to HTTP 404 in the global exception handler.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Maps to HTTP 409 in the global exception handler.</summary>
public sealed class ConflictException(string message) : Exception(message);

/// <summary>Maps to HTTP 401 in the global exception handler.</summary>
public sealed class AuthenticationFailedException(string message) : Exception(message);

/// <summary>Maps to HTTP 423 (locked) in the global exception handler.</summary>
public sealed class AccountLockedException(string message) : Exception(message);
