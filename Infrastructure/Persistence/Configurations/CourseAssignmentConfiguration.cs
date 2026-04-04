using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class CourseAssignmentConfiguration : IEntityTypeConfiguration<CourseAssignmentEntity>
    {
        public void Configure(EntityTypeBuilder<CourseAssignmentEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.CourseId)
                .IsRequired();

            builder.Property(x => x.AssignedToUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.AssignedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.AssignedAt)
                .IsRequired();


            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Course)
                .WithMany(x => x.CourseAssignments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AssignedToUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.TenantId, x.AssignedToUserId, x.CourseId })
                .HasDatabaseName("IX_CourseAssignments_TenantId_AssignedToUserId_CourseId_Status");
        }
    }
}
