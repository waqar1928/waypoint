using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Waypoint.Common;

namespace Waypoint.Api;

/// <summary>
/// Maps every exception type thrown by a module's Application layer to the
/// RFC 7807 Problem Details envelope described in docs/05-api-contract.md.
/// Handlers never format HTTP responses themselves — this is the one place
/// domain/application exceptions become status codes.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validationException => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/validation-failed",
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more fields are invalid.",
                Extensions = { ["errors"] = ToErrorDictionary(validationException) },
            },
            NotFoundException notFound => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/not-found",
                Title = "Not found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFound.Message,
            },
            ConflictException conflict => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/conflict",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = conflict.Message,
            },
            AuthenticationFailedException authFailed => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/authentication-failed",
                Title = "Authentication failed",
                Status = StatusCodes.Status401Unauthorized,
                Detail = authFailed.Message,
            },
            AccountLockedException locked => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/account-locked",
                Title = "Account locked",
                Status = StatusCodes.Status423Locked,
                Detail = locked.Message,
            },
            AntiforgeryValidationException => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/csrf-token-invalid",
                Title = "Invalid or missing CSRF token",
                Status = StatusCodes.Status403Forbidden,
            },
            AiServiceUnavailableException aiUnavailable => new ProblemDetails
            {
                Type = "https://waypoint.app/errors/ai-service-unavailable",
                Title = "Waypoint Coach is unavailable",
                Status = StatusCodes.Status503ServiceUnavailable,
                Detail = aiUnavailable.Message,
            },
            _ => null,
        };

        if (problem is null)
        {
            logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
            problem = new ProblemDetails
            {
                Type = "https://waypoint.app/errors/unexpected",
                Title = "An unexpected error occurred",
                Status = StatusCodes.Status500InternalServerError,
            };
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problem.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static Dictionary<string, string[]> ToErrorDictionary(ValidationException exception) =>
        exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => char.ToLowerInvariant(g.Key[0]) + g.Key[1..], g => g.Select(e => e.ErrorMessage).ToArray());
}
