using Microsoft.EntityFrameworkCore;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Infrastructure;

public sealed class MentorshipDbContext(DbContextOptions<MentorshipDbContext> options) : DbContext(options)
{
    public DbSet<MentorProfile> MentorProfiles => Set<MentorProfile>();
    public DbSet<HelpRequest> HelpRequests => Set<HelpRequest>();
    public DbSet<HelpRequestResponse> HelpRequestResponses => Set<HelpRequestResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MentorshipDbContext).Assembly);
    }
}
