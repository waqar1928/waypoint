using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.Journal.Application;

namespace Waypoint.Journal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddJournalModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<JournalDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IJournalRepository, JournalRepository>();
        services.AddScoped<IJournalSummaryProvider, JournalSummaryProvider>();
        services.AddScoped<IStartupMigrator, JournalStartupMigrator>();

        return services;
    }
}
