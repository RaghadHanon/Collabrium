using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;
public sealed class EventConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.StartAt)
            .IsRequired();

        builder.Property(e => e.EndAt)
            .IsRequired();

        builder.Property(e => e.IsOnline)
            .IsRequired();

        builder.Property(e => e.Location)
            .HasMaxLength(500);

        builder.Property(e => e.MeetingUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.MaxAttendees)
            .IsRequired(false);

        builder.Property(e => e.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(e => e.IsCancelled)
            .IsRequired();

        builder.OwnsOne(e => e.BannerImage, image =>
        {
            image.Property(i => i.Url)
                .HasColumnName("BannerImageUrl")
                .HasMaxLength(1000);

            image.Property(i => i.PublicId)
                .HasColumnName("BannerImagePublicId")
                .HasMaxLength(500);
        });

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Attendances)
            .WithOne(a => a.Event)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.ReminderSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.ReminderSentAt)
            .IsRequired(false);

        builder.HasIndex(e => new { e.TenantId, e.StartAt });

        builder.HasIndex(e => new { e.TenantId, e.EndAt });

        builder.HasIndex(e => new { e.TenantId, e.IsCancelled });

        builder.HasIndex(e => new { e.ReminderSent, e.StartAt, e.IsCancelled });
    }
}
