using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ResourceConfiguration : IEntityTypeConfiguration<ResourceEntity>
    {
        public void Configure(EntityTypeBuilder<ResourceEntity> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Url)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.Type)
                .IsRequired();

            builder.Property(r => r.Order)
                .IsRequired();

            builder.HasIndex(r => new { r.SubTopicId, r.Order })
                .HasDatabaseName("IX_Resource_SubTopic_Order");

            builder.HasOne(r => r.SubTopic)
                .WithMany(s => s.Resources)
                .HasForeignKey(r => r.SubTopicId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
