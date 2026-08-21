using Xunit;

// WaypointApiFactory sets ConnectionStrings__Postgres (and other config) via
// Environment.SetEnvironmentVariable in ConfigureWebHost — necessarily process-wide, not scoped to
// one factory instance, because every module's AddXxxModule(configuration) reads the connection
// string eagerly at service-registration time (see WaypointApiFactory's own doc comment on why
// in-memory configuration sources arrive too late). Each integration test CLASS gets its own
// WaypointApiFactory (IClassFixture is per-class, not shared across classes) and spins up its own
// throwaway Postgres container - safe in isolation, but xUnit runs different test classes in
// parallel by default, so two classes' factories were racing to set the SAME environment variable
// to two DIFFERENT containers' connection strings. This surfaced as intermittent 500s from
// completely unrelated endpoints (e.g. /api/v1/auth/register) the moment a second integration test
// class (PushNotificationFlowTests) was added — confirmed empirically: both classes' factories
// correctly started isolated Testcontainers instances (see the diagnostics log), so the failure
// was never about the database itself, only about which container's connection string won the
// race for the shared environment variable at host-startup time.
//
// Disabling parallelization for this assembly is the correct fix, not a workaround: these tests
// already can't safely share resources with each other (each owns a real Postgres container and
// mutates process state), so running them serially is what "safe" always meant here, it just
// hadn't been exercised with more than one class before.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
