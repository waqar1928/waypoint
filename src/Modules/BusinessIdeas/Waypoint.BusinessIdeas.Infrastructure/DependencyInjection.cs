using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Common;
using Waypoint.Common.Infrastructure;
using Waypoint.BusinessIdeas.Application;

namespace Waypoint.BusinessIdeas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessIdeasModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres configuration.");

        services.AddDbContext<BusinessIdeasDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(new AuditableEntitySaveChangesInterceptor()));

        services.AddScoped<IBusinessIdeasRepository, BusinessIdeasRepository>();
        services.AddScoped<IStartupMigrator, BusinessIdeasStartupMigrator>();
        services.AddSingleton<IViabilityEstimateGenerator, HeuristicViabilityEstimateGenerator>();
        services.AddScoped<IBusinessIdeaSummaryProvider, BusinessIdeaSummaryProvider>();

        return services;
    }
}
