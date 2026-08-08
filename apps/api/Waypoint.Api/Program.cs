using System.Reflection;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Waypoint.Api;
using Waypoint.Audit.Infrastructure;
using Waypoint.Common;
using Waypoint.Identity.Api;
using Waypoint.Identity.Infrastructure;
using Waypoint.Users.Api;
using Waypoint.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ---- Modules ---------------------------------------------------------
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddAuditModule(builder.Configuration);

// ---- Cross-cutting ----------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var applicationAssemblies = new[]
{
    Assembly.Load("Waypoint.Identity.Application"),
    Assembly.Load("Waypoint.Users.Application"),
};

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(applicationAssemblies);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblies(applicationAssemblies);

builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "waypoint.csrf";
    options.Cookie.HttpOnly = false; // the frontend must be able to read this to echo it back as a header
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var webAppBaseUrl = builder.Configuration["Waypoint:WebAppBaseUrl"]
    ?? throw new InvalidOperationException("Missing Waypoint:WebAppBaseUrl configuration.");

builder.Services.AddCors(options =>
    options.AddPolicy("WebApp", policy => policy
        .WithOrigins(webAppBaseUrl)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 10,
            QueueLimit = 0,
        }));

    options.AddPolicy("api", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = 100,
            QueueLimit = 0,
        }));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors("WebApp");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<AntiforgeryValidationMiddleware>();
app.UseAuthorization();

app.MapAntiforgeryEndpoints();
app.MapIdentityEndpoints();
app.MapUsersEndpoints();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

if (builder.Configuration.GetValue<bool>("Waypoint:AutoMigrate"))
{
    using var scope = app.Services.CreateScope();
    foreach (var migrator in scope.ServiceProvider.GetServices<IStartupMigrator>())
    {
        await migrator.MigrateAsync(CancellationToken.None);
    }
}

app.Run();

/// <summary>Validates the CSRF double-submit token on every mutating /api/v1 request (see docs/05-api-contract.md).</summary>
public sealed class AntiforgeryValidationMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    private static readonly HashSet<string> MutatingMethods = ["POST", "PUT", "PATCH", "DELETE"];

    public async Task InvokeAsync(HttpContext context)
    {
        var isMutatingApiCall = context.Request.Path.StartsWithSegments("/api/v1")
            && MutatingMethods.Contains(context.Request.Method)
            && !context.Request.Path.StartsWithSegments("/api/v1/antiforgery");

        if (isMutatingApiCall)
        {
            await antiforgery.ValidateRequestAsync(context);
        }

        await next(context);
    }
}

public partial class Program;
