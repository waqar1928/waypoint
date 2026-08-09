using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.Mentorship.Application;

namespace Waypoint.Mentorship.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMentorshipModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<MentorshipDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IMentorshipRepository, MentorshipRepository>();
        services.AddScoped<IStartupMigrator, MentorshipStartupMigrator>();

        return services;
    }
}
