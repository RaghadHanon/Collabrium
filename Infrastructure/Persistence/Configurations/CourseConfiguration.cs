using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<CourseEntity>
{
    public void Configure(EntityTypeBuilder<CourseEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.InstructorName)
            .HasMaxLength(150);

        builder.Property(x => x.Level)
            .IsRequired();

        builder.Property(x => x.Language)
            .IsRequired();

        builder.Property(x => x.EstimatedDurationInHours);

        builder.OwnsOne(x => x.ThumbnailImage, image =>
        {
            image.Property(i => i.Url)
                .HasColumnName("ThumbnailImage_Url")
                .HasMaxLength(1000);

            image.Property(i => i.PublicId)
                .HasColumnName("ThumbnailImage_PublicId")
                .HasMaxLength(500);
        });

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Tags)
       .WithMany()
       .UsingEntity(j => j.ToTable("CourseTags"));

        builder.HasMany(x => x.UserCourses)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CourseRatings)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(c => c.LearningPoints, lp =>
        {
            lp.ToTable("CourseLearningPoints");

            lp.WithOwner().HasForeignKey("CourseId");

            lp.Property<int>("Id");
            lp.HasKey("Id");

            lp.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(200);

            lp.HasIndex("CourseId");
        });

        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_Courses_Title");

        builder.HasIndex(x => new { x.TenantId, x.Title })
            .HasDatabaseName("IX_Courses_TenantId_Title");
    }
}
