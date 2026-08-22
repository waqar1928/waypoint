using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Testcontainers.PostgreSql;
using Waypoint.Identity.Application;
using Xunit;

namespace Waypoint.Api.IntegrationTests;

/// <summary>
/// Spins up a real Postgres to run against (see docs/07-technical-architecture.md
/// "CI-ready") so these tests exercise real EF Core migrations and the real
/// Identity/Users modules — no mocks, no in-memory provider standing in for
/// Postgres-specific behavior.
///
/// Tries Testcontainers first (a throwaway Postgres container per test run) since
/// that's the more hermetic, CI-friendly option. Falls back to a scratch database on
/// whatever local Postgres server the rest of this project's live verification
/// already uses (see apps/api/Waypoint.Api/appsettings.json) when Testcontainers
/// isn't usable — which is the normal case for local development environments that
/// don't happen to have Docker installed. The fallback creates
/// "waypoint_integration_test" fresh (drop-if-exists, then create) before each test
/// run and drops it again on disposal, so it never touches the "waypoint" dev
/// database used for manual verification.
///
/// Both candidates are actively connection-tested here, before this method returns —
/// not assumed to work just because container startup or database creation didn't
/// throw. The first real bug this whole fallback design ever hit (Phase 12) was
/// exactly this gap: Testcontainers failed silently in a way this class swallowed,
/// fell back to a hardcoded "localhost:5432" that's only valid in the sandbox this
/// project's live verification runs in, and the resulting connection failure didn't
/// surface until deep inside Program.cs's migration code on a completely unrelated
/// stack trace — instead of a clear, actionable error right here.
/// </summary>
public sealed class WaypointApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string LocalAdminConnectionString =
        "Host=localhost;Port=5432;Database=postgres;Username=waypoint;Password=waypoint_dev_password";
    private const string LocalTestDatabaseName = "waypoint_integration_test";

    private PostgreSqlContainer? _postgresContainer;
    private bool _usingContainer;
    private string _connectionString = string.Empty;

    // Deliberately NOT relying on ConfigureWebHost's ConfigureAppConfiguration(AddInMemoryCollection)
    // to inject the connection string — this is the real bug the last three CI failures traced back
    // to. Every module's AddXxxModule(configuration) extension method reads the Postgres connection
    // string EAGERLY at service-registration time (`configuration.GetConnectionString("Postgres")`,
    // captured into a closure passed to `UseNpgsql(...)`), and that registration happens as part of
    // Program.cs's own top-level code, executing BEFORE WebApplicationFactory's DeferredHostBuilder
    // folds this class's ConfigureAppConfiguration override into `builder.Configuration`. The
    // override was silently arriving too late on every run — confirmed via file-based diagnostics
    // (see Log below) that Testcontainers itself was working the whole time, correctly reporting
    // its real (random) mapped port, while the app still connected to the literal dev value baked
    // into appsettings.json. Environment variables don't have this problem: they're included in
    // WebApplication.CreateBuilder(args)'s configuration sources from the very first line of
    // Program.cs, so setting one here (in InitializeAsync, guaranteed by xUnit's IAsyncLifetime to
    // run before any test can trigger Program.Main() via CreateClient()) is visible in time for
    // every module's eager connection-string read.
    /// <summary>
    /// Swapped in for the real IEmailSender so tests can complete the real email-verification
    /// flow (register → read the real token out of the captured email → confirm → login) without
    /// a real inbox. See CapturingEmailSender's own doc comment for why this exists — added once
    /// RequireConfirmedAccount started actually blocking login for unconfirmed accounts.
    /// </summary>
    public CapturingEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", _connectionString);
        Environment.SetEnvironmentVariable("Waypoint__AutoMigrate", "true");

        // Every request from this in-process TestServer reports the same loopback IP, so all
        // tests in this suite share one "auth" rate-limit partition (see Program.cs's new
        // Waypoint:RateLimits:Auth config option, added specifically for this). 200 is generous
        // headroom for the whole suite's auth-flow tests without weakening the real production
        // default (still 10/minute unless explicitly overridden) even slightly.
        Environment.SetEnvironmentVariable("Waypoint__RateLimits__Auth", "200");

        // A plain ConfigureServices callback here still runs after Program.cs's own DI
        // registrations (WebApplicationFactory guarantees ConfigureWebHost's callbacks are the
        // last word), so RemoveAll+re-add reliably overrides the real registration — unlike the
        // connection-string case above, this isn't subject to the eager-read timing gotcha, since
        // IEmailSender is only ever resolved from the container when a handler actually needs it
        // (well after the host has fully started), not captured into a closure at registration
        // time. (Microsoft.AspNetCore.TestHost's ConfigureTestServices would be the more
        // conventional spelling of this, but isn't referenced by this test project.)
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    // Diagnostic trail written to a file, not just Console — xUnit's own output capture around a
    // class fixture's IAsyncLifetime methods has proven unreliable at surfacing Console.WriteLine
    // in the "dotnet test" console logger (confirmed empirically: two prior CI failures showed no
    // trace of this class's own diagnostic lines at all, success or failure, even though the
    // logic must have run for the process to reach the connection string it ultimately used). The
    // CI workflow prints this file unconditionally after the test step so nothing gets swallowed.
    //
    // Hardcoded to /tmp rather than Path.GetTempPath() deliberately: on the GitHub Actions
    // ubuntu-latest runner this phase targets, they're the same thing, but Path.GetTempPath()
    // reads $TMPDIR, which some sandboxed local dev environments override to something other than
    // /tmp — verified empirically that this was silently breaking local verification of this same
    // diagnostic path before this fix.
    private const string DiagnosticLogPath = "/tmp/waypoint-integration-test-factory-diagnostics.log";

    private static void Log(string message)
    {
        var line = $"[{DateTimeOffset.UtcNow:O}] {message}";
        Console.Error.WriteLine(line);
        File.AppendAllText(DiagnosticLogPath, line + Environment.NewLine);
    }

    public async Task InitializeAsync()
    {
        Log("InitializeAsync starting.");

        var containerFailure = await TryInitializeContainerAsync();
        if (containerFailure is null)
        {
            Log($"Using Testcontainers. Connection string host/port: {DescribeConnectionString(_connectionString)}");
            return;
        }

        Log($"Testcontainers unavailable or unreachable ({containerFailure}) — falling back to local Postgres database '{LocalTestDatabaseName}'.");

        var localFailure = await TryInitializeLocalFallbackAsync();
        if (localFailure is null)
        {
            Log($"Using local Postgres fallback. Connection string host/port: {DescribeConnectionString(_connectionString)}");
            return;
        }

        Log($"Local fallback also failed: {localFailure}");

        throw new InvalidOperationException(
            "WaypointApiFactory could not reach a Postgres instance to run integration tests against. " +
            $"Testcontainers attempt failed: {containerFailure}. " +
            $"Local Postgres fallback (host=localhost, port=5432) also failed: {localFailure}. " +
            "Either make a Docker daemon available (Testcontainers) or run a local Postgres server " +
            "matching apps/api/Waypoint.Api/appsettings.json's dev credentials.");
    }

    /// <summary>Host/port only — never logs the password.</summary>
    private static string DescribeConnectionString(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return $"{builder.Host}:{builder.Port}/{builder.Database}";
        }
        catch (Exception ex)
        {
            return $"<unparseable: {ex.Message}>";
        }
    }

    /// <returns>null on success; a diagnostic message on failure (never throws).</returns>
    private async Task<string?> TryInitializeContainerAsync()
    {
        try
        {
            // Building the container (not just starting it) is what actually talks to the Docker
            // daemon, so it has to be inside this try block too — done eagerly in a field
            // initializer, it would throw before this method ever got a chance to catch it.
            // Image passed to the constructor rather than via .WithImage(): the parameterless
            // constructor is obsolete as of Testcontainers 4.14 and is slated for removal.
            // Kept pinned to the same tag docker-compose.yml uses, so tests exercise the same
            // Postgres major version as dev and production.
            var container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("waypoint_test")
                .WithUsername("waypoint")
                .WithPassword("waypoint_test_password")
                .Build();

            await container.StartAsync();
            var connectionString = container.GetConnectionString();
            Log($"Container started. Reported connection string host/port: {DescribeConnectionString(connectionString)}");

            // Don't trust "StartAsync didn't throw" as proof the container is actually reachable —
            // confirm with a real connection before committing to this as the test database.
            await using (var probe = new NpgsqlConnection(connectionString))
            {
                await probe.OpenAsync();
            }
            Log("Probe connection to container succeeded.");

            _postgresContainer = container;
            _connectionString = connectionString;
            _usingContainer = true;
            return null;
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    /// <returns>null on success; a diagnostic message on failure (never throws).</returns>
    private async Task<string?> TryInitializeLocalFallbackAsync()
    {
        try
        {
            await RecreateLocalTestDatabaseAsync();
            var connectionString =
                $"Host=localhost;Port=5432;Database={LocalTestDatabaseName};Username=waypoint;Password=waypoint_dev_password";
            Log("Local scratch database recreated successfully.");

            await using (var probe = new NpgsqlConnection(connectionString))
            {
                await probe.OpenAsync();
            }
            Log("Probe connection to local fallback succeeded.");

            _usingContainer = false;
            _connectionString = connectionString;
            return null;
        }
        catch (Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    private static async Task RecreateLocalTestDatabaseAsync()
    {
        await using var adminConnection = new NpgsqlConnection(LocalAdminConnectionString);
        await adminConnection.OpenAsync();

        // Terminate any lingering connections from a previous run before dropping — a stray
        // connection (e.g. from a test that crashed mid-run) would otherwise block DROP DATABASE.
        await using (var terminate = new NpgsqlCommand(
            $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{LocalTestDatabaseName}' AND pid <> pg_backend_pid();",
            adminConnection))
        {
            await terminate.ExecuteNonQueryAsync();
        }

        await using (var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS {LocalTestDatabaseName};", adminConnection))
        {
            await drop.ExecuteNonQueryAsync();
        }

        await using var create = new NpgsqlCommand($"CREATE DATABASE {LocalTestDatabaseName};", adminConnection);
        await create.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_usingContainer && _postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
        else if (!_usingContainer && !string.IsNullOrEmpty(_connectionString))
        {
            try
            {
                await RecreateLocalTestDatabaseAsync(); // leaves a clean, empty DB behind rather than a dirty one
            }
            catch
            {
                // Best-effort cleanup — don't fail the test run over it.
            }
        }

        await base.DisposeAsync();
    }
}
