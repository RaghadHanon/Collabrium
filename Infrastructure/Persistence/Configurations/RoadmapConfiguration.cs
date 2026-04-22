using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoadmapConfiguration : IEntityTypeConfiguration<RoadmapEntity>
    {
        public void Configure(EntityTypeBuilder<RoadmapEntity> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(r => r.Description)
                .HasMaxLength(1000);

            builder.Property(r => r.TenantId)
           .IsRequired()
           .HasMaxLength(450);

            builder.Property(r => r.CreatedByUserId)
            .HasMaxLength(450);

            builder.HasIndex(r => r.Title)
             .HasDatabaseName("IX_Roadmap_Title");

            builder.HasOne(r => r.Tenant)
                .WithMany()
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.CreatedByUser)
           .WithMany()
           .HasForeignKey(r => r.CreatedByUserId)
           .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(r => r.Nodes)
            .WithOne(n => n.Roadmap)
            .HasForeignKey(n => n.RoadmapId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Tags)
               .WithMany()
               .UsingEntity(j => j.ToTable("RoadmapTags"));
        }
    }
}
