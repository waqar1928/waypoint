using System.Reflection;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Waypoint.Actions.Api;
using Waypoint.Actions.Infrastructure;
using Waypoint.AI.Api;
using Waypoint.AI.Infrastructure;
using Waypoint.Api;
using Waypoint.Audit.Api;
using Waypoint.Audit.Infrastructure;
using Waypoint.BusinessIdeas.Api;
using Waypoint.BusinessIdeas.Infrastructure;
using Waypoint.Common;
using Waypoint.Community.Api;
using Waypoint.Community.Infrastructure;
using Waypoint.Dreams.Api;
using Waypoint.Dreams.Infrastructure;
using Waypoint.Experiments.Api;
using Waypoint.Experiments.Infrastructure;
using Waypoint.Goals.Api;
using Waypoint.Goals.Infrastructure;
using Waypoint.Identity.Api;
using Waypoint.Identity.Infrastructure;
using Waypoint.Journal.Api;
using Waypoint.Journal.Infrastructure;
using Waypoint.Mentorship.Api;
using Waypoint.Mentorship.Infrastructure;
using Waypoint.Notifications.Api;
using Waypoint.Notifications.Infrastructure;
using Waypoint.Users.Api;
using Waypoint.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Application", "Waypoint.Api");

    // Plain text in dev (readable at a terminal while iterating); structured JSON
    // (CLEF/compact) everywhere else, since that's what a real log aggregator (CloudWatch,
    // Datadog, Grafana Loki, etc.) needs to index and query fields instead of grepping text.
    // appsettings.json's Serilog:WriteTo still controls sinks/levels — this only controls how
    // the console sink renders each event, and only for the console sink specifically.
    if (context.HostingEnvironment.IsDevelopment())
    {
        configuration.WriteTo.Console();
    }
    else
    {
        configuration.WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter());
    }
});

// ---- Modules ---------------------------------------------------------
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddAuditModule(builder.Configuration);
builder.Services.AddDreamsModule(builder.Configuration);
builder.Services.AddJournalModule(builder.Configuration);
builder.Services.AddGoalsModule(builder.Configuration);
builder.Services.AddActionsModule(builder.Configuration);
builder.Services.AddExperimentsModule(builder.Configuration);
builder.Services.AddBusinessIdeasModule(builder.Configuration);
builder.Services.AddAiModule(builder.Configuration);
builder.Services.AddCommunityModule(builder.Configuration);
builder.Services.AddMentorshipModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

// ---- Cross-cutting ----------------------------------------------------
// In-process only (not distributed) — sole current consumer is AnthropicAiService's prompt
// template lookup, a single-instance dev/staging deployment. Revisit with a distributed cache
// (Redis) if this ever runs multi-instance and needs cache coherency across processes.
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
builder.Services.AddSingleton<IProductAnalyticsSink, LoggingProductAnalyticsSink>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase)));

var applicationAssemblies = new[]
{
    Assembly.Load("Waypoint.Identity.Application"),
    Assembly.Load("Waypoint.Users.Application"),
    Assembly.Load("Waypoint.Audit.Application"),
    Assembly.Load("Waypoint.Dreams.Application"),
    Assembly.Load("Waypoint.Journal.Application"),
    Assembly.Load("Waypoint.Goals.Application"),
    Assembly.Load("Waypoint.Actions.Application"),
    Assembly.Load("Waypoint.Experiments.Application"),
    Assembly.Load("Waypoint.BusinessIdeas.Application"),
    Assembly.Load("Waypoint.AI.Application"),
    Assembly.Load("Waypoint.Community.Application"),
    Assembly.Load("Waypoint.Mentorship.Application"),
    Assembly.Load("Waypoint.Notifications.Application"),
};

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(applicationAssemblies);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblies(applicationAssemblies);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(Waypoint.Common.Roles.Admin));
    // Admin can do everything Moderator can (superset) — see
    // docs/PRODUCTION_READINESS_AUDIT.md's Authorization section. Only the moderation-queue and
    // mentor-verification endpoint groups use this policy; every other /api/v1/admin/* group
    // (user lock/unlock, dream oversight, AI usage, the full audit log) still requires "Admin".
    options.AddPolicy("Moderator", policy =>
        policy.RequireRole(Waypoint.Common.Roles.Admin, Waypoint.Common.Roles.Moderator));
});

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
        // Origin restriction above is the load-bearing control (this API is only ever meant to be
        // called by the Next.js BFF, and server-to-server calls aren't subject to CORS at all —
        // this policy only matters for a browser trying to hit the API directly). Narrowed from
        // AllowAnyHeader/AllowAnyMethod to the exact set apps/web's proxy.ts and api-client.ts
        // actually send, per docs/PRODUCTION_READINESS_AUDIT.md's security section: every
        // AllowAny* needs a documented reason, and here there wasn't one.
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders("Content-Type", "X-CSRF-TOKEN")
        .AllowCredentials()));

// Permit limits are configurable (Waypoint:RateLimits:Auth/Api/Ai) rather than hardcoded, with
// these exact values as the defaults if unset — production behavior is unchanged unless an
// operator explicitly overrides one. This was added specifically because the integration test
// suite's WaypointApiFactory needs a higher "auth" limit than production: every request in that
// in-process TestServer reports the same loopback IP, so a growing number of legitimate
// multi-step auth flows (register → verify → login, etc.) sharing one partition key started
// tripping the real 10/minute production limit — a genuine test-infrastructure consequence of
// this pass's real email-verification-gate work, not a reason to weaken the production default.
var authRateLimit = builder.Configuration.GetValue("Waypoint:RateLimits:Auth", 10);
var apiRateLimit = builder.Configuration.GetValue("Waypoint:RateLimits:Api", 100);
var aiRateLimit = builder.Configuration.GetValue("Waypoint:RateLimits:Ai", 20);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = authRateLimit,
            QueueLimit = 0,
        }));

    options.AddPolicy("api", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = apiRateLimit,
            QueueLimit = 0,
        }));

    // Stricter than "api" — every request here is a billed AI call, not just a DB read/write.
    options.AddPolicy("ai", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = aiRateLimit,
            QueueLimit = 0,
        }));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres", tags: ["ready"]);

var app = builder.Build();

// Kept dev-only-conditional rather than unconditional: local dev runs Kestrel on plain HTTP
// (see launchSettings.json / --urls http://localhost:5080) with no dev HTTPS cert configured for
// this API, so an unconditional UseHttpsRedirection() would break every local/live-verification
// workflow. A production deployment either terminates TLS at Kestrel directly (this branch
// applies) or at a reverse proxy/load balancer that already enforces HTTPS before traffic reaches
// this process (this branch is then a harmless no-op, since the request already arrives as HTTPS).
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

// This API is only ever consumed by the Waypoint BFF (never renders HTML directly), but these
// headers are cheap, standard defense-in-depth and protect the Problem Details/JSON error bodies
// from being sniffed as HTML or framed by anything that did somehow get pointed at this origin.
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    // Same value GlobalExceptionHandler already puts in every Problem Details error body's
    // "traceId" field — surfaced here as a header too so it's available on every response
    // (success or failure), not just error bodies, for support/debugging workflows that want to
    // reference a specific request without needing to parse a JSON error payload first.
    context.Response.Headers.Append("X-Correlation-Id", context.TraceIdentifier);
    await next();
});

app.UseCors("WebApp");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<AntiforgeryValidationMiddleware>();
app.UseAuthorization();

app.MapAntiforgeryEndpoints();
app.MapIdentityEndpoints();
app.MapUsersEndpoints();
app.MapDreamsEndpoints();
app.MapJournalEndpoints();
app.MapGoalsEndpoints();
app.MapActionsEndpoints();
app.MapExperimentsEndpoints();
app.MapBusinessIdeasEndpoints();
app.MapAiEndpoints();
app.MapCommunityEndpoints();
app.MapMentorshipEndpoints();
app.MapAuditEndpoints();
app.MapNotificationsEndpoints();

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
