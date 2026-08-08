using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Waypoint.Identity.Infrastructure;

public sealed class WaypointIdentityDbContext(DbContextOptions<WaypointIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user =>
        {
            user.ToTable("identity_users");
        });
        builder.Entity<IdentityRole<Guid>>(role => role.ToTable("identity_roles"));
        builder.Entity<IdentityUserRole<Guid>>(ur => ur.ToTable("identity_user_roles"));
        builder.Entity<IdentityUserClaim<Guid>>(uc => uc.ToTable("identity_user_claims"));
        builder.Entity<IdentityUserLogin<Guid>>(ul => ul.ToTable("identity_user_logins"));
        builder.Entity<IdentityUserToken<Guid>>(ut => ut.ToTable("identity_user_tokens"));
        builder.Entity<IdentityRoleClaim<Guid>>(rc => rc.ToTable("identity_role_claims"));
    }
}
