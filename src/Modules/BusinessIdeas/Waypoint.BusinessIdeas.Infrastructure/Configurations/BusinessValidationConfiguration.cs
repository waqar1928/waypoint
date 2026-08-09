using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Infrastructure.Configurations;

public sealed class BusinessValidationConfiguration : IEntityTypeConfiguration<BusinessValidation>
{
    public void Configure(EntityTypeBuilder<BusinessValidation> builder)
    {
        builder.ToTable("business_validations", t =>
            t.HasCheckConstraint(
                "ck_business_validations_viability_estimate_range",
                "viability_estimate BETWEEN 0 AND 100"));
        builder.HasKey(v => v.Id);

        builder.Property(v => v.StrongAssumptions)
            .HasColumnType("jsonb")
            .HasConversion(JsonStringListConverter.Converter, JsonStringListConverter.Comparer);
        builder.Property(v => v.WeakAssumptions)
            .HasColumnType("jsonb")
            .HasConversion(JsonStringListConverter.Converter, JsonStringListConverter.Comparer);
        builder.Property(v => v.Unknowns)
            .HasColumnType("jsonb")
            .HasConversion(JsonStringListConverter.Converter, JsonStringListConverter.Comparer);
        builder.Property(v => v.RecommendedExperiments)
            .HasColumnType("jsonb")
            .HasConversion(JsonStringListConverter.Converter, JsonStringListConverter.Comparer);

        builder.HasIndex(v => v.BusinessIdeaId);
        builder.UseXminConcurrencyToken();
    }
}
