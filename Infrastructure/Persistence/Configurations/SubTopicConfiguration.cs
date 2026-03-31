using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SubTopicConfiguration : IEntityTypeConfiguration<SubTopicEntity>
    {
        public void Configure(EntityTypeBuilder<SubTopicEntity> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .HasMaxLength(2000);

            builder.Property(s => s.Order)
                .IsRequired();

            builder.HasIndex(s => new { s.TopicNodeId, s.Order })
                .IsUnique()
                .HasDatabaseName("IX_SubTopic_TopicNode_Order");

            builder.HasMany(s => s.Resources)
                .WithOne(r => r.SubTopic)
                .HasForeignKey(r => r.SubTopicId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
