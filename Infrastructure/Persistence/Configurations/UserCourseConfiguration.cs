using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class UserCourseConfiguration : IEntityTypeConfiguration<UserCourseEntity>
    {
        public void Configure(EntityTypeBuilder<UserCourseEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.CourseId)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.IsSaved)
                .IsRequired();

            builder.Property(x => x.StartedAt);

            builder.Property(x => x.CompletedAt);

            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Course)
                .WithMany(x => x.UserCourses)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.TenantId, x.UserId, x.CourseId })
                .IsUnique()
                .HasDatabaseName("IX_UserCourses_TenantId_UserId_CourseId");
        }
    }
}
