using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReferenceNodeConfiguration : IEntityTypeConfiguration<ReferenceNodeEntity>
    {
        public void Configure(EntityTypeBuilder<ReferenceNodeEntity> builder)
        {
            builder.Property(r => r.RefRoadmapId)
                .IsRequired();

            builder.HasOne(r => r.RefRoadmap)
                .WithMany()
                .HasForeignKey(r => r.RefRoadmapId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.RefRoadmapId)
                .HasDatabaseName("IX_ReferenceNode_RefRoadmapId");
        }
    }
}
