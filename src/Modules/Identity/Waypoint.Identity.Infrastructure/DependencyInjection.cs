using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Identity.Application;

namespace Waypoint.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<WaypointIdentityDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                // A real user must prove they own the email address before they can log in — see
                // docs/PRODUCTION_READINESS_AUDIT.md's Authentication section. Registration and
                // password-reset flows already work unaffected by this; it only gates login.
                // SignInManager.PasswordSignInAsync's PreSignInCheck enforces this by returning
                // SignInResult.NotAllowed (mapped to SignInOutcome.EmailNotConfirmed in
                // IdentityService.PasswordSignInAsync) once the password has already checked out
                // correctly for an unconfirmed account.
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<WaypointIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "waypoint.auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            // SameAsRequest (not Always) so local HTTP dev keeps working; a production deployment
            // that terminates behind HTTPS will still get Secure cookies since the request is HTTPS.
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.Configure<IdentityLinkOptions>(configuration.GetSection("Waypoint"));
        services.AddScoped<IIdentityService, IdentityService>();

        // Only registers real SMTP delivery once an operator explicitly configures a mail host —
        // see SmtpEmailSender's own doc comment for why this stays opt-in rather than defaulting
        // to "on". A deployment with nothing configured keeps the safe logging behavior instead of
        // silently failing every send.
        services.Configure<SmtpOptions>(configuration.GetSection("Email:Smtp"));
        if (!string.IsNullOrWhiteSpace(configuration["Email:Smtp:Host"]))
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, LoggingEmailSender>();
        }

        services.AddScoped<IStartupMigrator, IdentityStartupMigrator>();
        services.AddScoped<IStartupMigrator, RoleSeeder>();

        return services;
    }
}
