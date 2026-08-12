using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Waypoint.Common;

namespace Waypoint.Identity.Infrastructure;

/// <summary>
/// Bootstraps the "Admin" and "Moderator" roles from config allowlists (Waypoint:AdminEmails,
/// Waypoint:ModeratorEmails) — there is no self-service "make me admin/moderator" path anywhere
/// in the product, by design (see docs/09 Phase 8 scoping notes, extended for Moderator in
/// docs/PRODUCTION_READINESS_AUDIT.md's Authorization section). Runs on every startup,
/// idempotently: ensures each role exists, then grants it to any configured email that doesn't
/// already have it. Adding a new admin or moderator is a config change plus a restart, nothing
/// more — no manual SQL, no code changes.
///
/// Renamed from AdminRoleSeeder (single-role) now that it seeds two roles — same class, same
/// registration point, no behavior change for existing Admin seeding.
/// </summary>
internal sealed class RoleSeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<RoleSeeder> logger) : IStartupMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await SeedRoleAsync(Roles.Admin, "Waypoint:AdminEmails");
        await SeedRoleAsync(Roles.Moderator, "Waypoint:ModeratorEmails");
    }

    private async Task SeedRoleAsync(string roleName, string configSectionKey)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        var emails = configuration.GetSection(configSectionKey).Get<string[]>() ?? [];
        foreach (var email in emails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.LogWarning("Configured {Role} email {Email} has no matching user yet — skipping.", roleName, email);
                continue;
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
                logger.LogInformation("Granted {Role} role to {Email}.", roleName, email);
            }
        }
    }
}
