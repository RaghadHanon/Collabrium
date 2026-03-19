using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class CourseTagConfiguration : IEntityTypeConfiguration<CourseTagEntity>
    {
        public void Configure(EntityTypeBuilder<CourseTagEntity> builder)
        {
            builder.HasKey(x => new { x.CourseId, x.TagId });

            builder.Property(x => x.CourseId)
                .IsRequired();

            builder.Property(x => x.TagId)
                .IsRequired();

            builder.HasOne(x => x.Course)
                .WithMany(x => x.CourseTags)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Tag)
                .WithMany()
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TagId)
                .HasDatabaseName("IX_CourseTags_TagId");
        }
    }
}
