using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waypoint.AI.Domain;

namespace Waypoint.AI.Infrastructure.Configurations;

public sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("ai_conversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Topic).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.HasIndex(c => new { c.UserId, c.UpdatedAt });
        builder.UseXminConcurrencyToken();
    }
}
