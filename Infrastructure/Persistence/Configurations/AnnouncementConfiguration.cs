using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;
public sealed class AnnouncementConfiguration : IEntityTypeConfiguration<AnnouncementEntity>
{
    public void Configure(EntityTypeBuilder<AnnouncementEntity> builder)
    {
        builder.ToTable("Announcements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(a => a.Priority)
            .IsRequired();

        builder.Property(a => a.PublishAt)
            .IsRequired();

        builder.Property(a => a.ExpiresAt)
            .IsRequired(false);

        builder.Property(a => a.NotificationSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.NotificationSentAt)
            .IsRequired(false);

        builder.Property(a => a.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CreatedByUser)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Audiences)
            .WithOne(a => a.Announcement)
            .HasForeignKey(a => a.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.TenantId, a.PublishAt });

        builder.HasIndex(a => new { a.TenantId, a.ExpiresAt });

        builder.HasIndex(a => new { a.TenantId, a.Priority });

        builder.HasIndex(a => new { a.NotificationSent, a.PublishAt, a.ExpiresAt });
    }
}