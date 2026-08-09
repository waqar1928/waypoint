using Microsoft.EntityFrameworkCore;
using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Infrastructure;

public sealed class BusinessIdeasDbContext(DbContextOptions<BusinessIdeasDbContext> options) : DbContext(options)
{
    public DbSet<BusinessIdea> BusinessIdeas => Set<BusinessIdea>();
    public DbSet<BusinessValidation> BusinessValidations => Set<BusinessValidation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessIdeasDbContext).Assembly);
    }
}
