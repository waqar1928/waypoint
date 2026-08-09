using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _connectionString,
                ["Waypoint:AutoMigrate"] = "true",
            });
        });
    }

    public async Task InitializeAsync()
    {
        var containerFailure = await TryInitializeContainerAsync();
        if (containerFailure is null)
        {
            return;
        }

        Console.Error.WriteLine(
            $"[WaypointApiFactory] Testcontainers unavailable or unreachable ({containerFailure}) — " +
            $"falling back to local Postgres database '{LocalTestDatabaseName}'.");

        var localFailure = await TryInitializeLocalFallbackAsync();
        if (localFailure is null)
        {
            return;
        }

        throw new InvalidOperationException(
            "WaypointApiFactory could not reach a Postgres instance to run integration tests against. " +
            $"Testcontainers attempt failed: {containerFailure}. " +
            $"Local Postgres fallback (host=localhost, port=5432) also failed: {localFailure}. " +
            "Either make a Docker daemon available (Testcontainers) or run a local Postgres server " +
            "matching apps/api/Waypoint.Api/appsettings.json's dev credentials.");
    }

    /// <returns>null on success; a diagnostic message on failure (never throws).</returns>
    private async Task<string?> TryInitializeContainerAsync()
    {
        try
        {
            // Building the container (not just starting it) is what actually talks to the Docker
            // daemon, so it has to be inside this try block too — done eagerly in a field
            // initializer, it would throw before this method ever got a chance to catch it.
            var container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("waypoint_test")
                .WithUsername("waypoint")
                .WithPassword("waypoint_test_password")
                .Build();

            await container.StartAsync();
            var connectionString = container.GetConnectionString();

            // Don't trust "StartAsync didn't throw" as proof the container is actually reachable —
            // confirm with a real connection before committing to this as the test database.
            await using (var probe = new NpgsqlConnection(connectionString))
            {
                await probe.OpenAsync();
            }

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

            await using (var probe = new NpgsqlConnection(connectionString))
            {
                await probe.OpenAsync();
            }

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
