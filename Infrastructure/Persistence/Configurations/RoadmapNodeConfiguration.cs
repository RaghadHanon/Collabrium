using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoadmapNodeConfiguration : IEntityTypeConfiguration<RoadmapNodeEntity>
    {
        public void Configure(EntityTypeBuilder<RoadmapNodeEntity> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Description)
                .HasMaxLength(2000);

            builder.Property(n => n.Order)
                .IsRequired();

            builder.HasOne(n => n.Roadmap)
           .WithMany(r => r.Nodes)
           .HasForeignKey(n => n.RoadmapId)
           .OnDelete(DeleteBehavior.Cascade);

            // TPH Inheritance
            builder
                .HasDiscriminator<string>("NodeType")
                .HasValue<TopicNodeEntity>("Topic")
                .HasValue<ReferenceNodeEntity>("Reference");
        }
    }
}
