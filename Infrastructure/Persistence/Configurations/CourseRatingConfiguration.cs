using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class CourseRatingConfiguration : IEntityTypeConfiguration<CourseRatingEntity>
    {
        public void Configure(EntityTypeBuilder<CourseRatingEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.CourseId)
                .IsRequired();

            builder.Property(x => x.Rating)
                .IsRequired();



            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Course)
                .WithMany(x => x.CourseRatings)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.UserId, x.CourseId })
                .IsUnique()
                .HasDatabaseName("IX_CourseRatings_TenantId_UserId_CourseId");
        }
    }
}
