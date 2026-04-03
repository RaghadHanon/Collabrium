using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoadmapTagConfiguration : IEntityTypeConfiguration<RoadmapTagEntity>
    {
        public void Configure(EntityTypeBuilder<RoadmapTagEntity> builder)
        {
            builder.HasKey(rt => new { rt.RoadmapId, rt.TagId });

            builder.HasOne(rt => rt.Roadmap)
                .WithMany(r => r.Tags)
                .HasForeignKey(rt => rt.RoadmapId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rt => rt.Tag)
                .WithMany(t => t.Roadmaps)
                .HasForeignKey(rt => rt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rt => rt.TagId)
                .HasDatabaseName("IX_RoadmapTag_TagId");
        }
    }
}
