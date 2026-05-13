using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TopicNodeConfiguration : IEntityTypeConfiguration<TopicNodeEntity>
{
    public void Configure(EntityTypeBuilder<TopicNodeEntity> builder)
    {
        builder.HasMany(t => t.SubTopics)
            .WithOne(st => st.TopicNode)
            .HasForeignKey(st => st.TopicNodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
