using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Waypoint.Common;

namespace Waypoint.Identity.Infrastructure;

/// <summary>
/// Bootstraps the "Admin" role from a config allowlist (Waypoint:AdminEmails) — there is no
/// self-service "make me admin" path anywhere in the product, by design (see docs/09 Phase 8
/// scoping notes). Runs on every startup, idempotently: ensures the role exists, then grants it
/// to any configured email that doesn't already have it. Adding a new admin is a config change
/// plus a restart, nothing more — no manual SQL, no code changes.
/// </summary>
internal sealed class AdminRoleSeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<AdminRoleSeeder> logger) : IStartupMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Admin));
        }

        var adminEmails = configuration.GetSection("Waypoint:AdminEmails").Get<string[]>() ?? [];
        foreach (var email in adminEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.LogWarning("Configured admin email {Email} has no matching user yet — skipping.", email);
                continue;
            }

            if (!await userManager.IsInRoleAsync(user, Roles.Admin))
            {
                await userManager.AddToRoleAsync(user, Roles.Admin);
                logger.LogInformation("Granted Admin role to {Email}.", email);
            }
        }
    }
}
