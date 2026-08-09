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
/// already uses (see apps/api/Waypoint.Api/appsettings.json) when no Docker daemon is
/// available — which is the normal case for local development environments that
/// don't happen to have Docker installed. The fallback creates
/// "waypoint_integration_test" fresh (drop-if-exists, then create) before each test
/// run and drops it again on disposal, so it never touches the "waypoint" dev
/// database used for manual verification.
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
            _postgresContainer = container;
            _connectionString = container.GetConnectionString();
            _usingContainer = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[WaypointApiFactory] Docker/Testcontainers unavailable ({ex.GetType().Name}: {ex.Message}) — " +
                $"falling back to local Postgres database '{LocalTestDatabaseName}'.");
            _usingContainer = false;
            await RecreateLocalTestDatabaseAsync();
            _connectionString =
                $"Host=localhost;Port=5432;Database={LocalTestDatabaseName};Username=waypoint;Password=waypoint_dev_password";
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
        else
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
