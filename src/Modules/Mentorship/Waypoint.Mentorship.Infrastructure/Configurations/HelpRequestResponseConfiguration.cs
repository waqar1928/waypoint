using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Infrastructure.Configurations;

public sealed class HelpRequestResponseConfiguration : IEntityTypeConfiguration<HelpRequestResponse>
{
    public void Configure(EntityTypeBuilder<HelpRequestResponse> builder)
    {
        builder.ToTable("help_request_responses");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Body).HasMaxLength(2000).IsRequired();

        builder.HasOne<HelpRequest>()
            .WithMany()
            .HasForeignKey(r => r.HelpRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.HelpRequestId);
        builder.UseXminConcurrencyToken();
    }
}
