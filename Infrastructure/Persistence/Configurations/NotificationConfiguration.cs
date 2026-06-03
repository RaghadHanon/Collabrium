using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(n => n.RecipientUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(n => n.TriggeredByUserId)
            .HasMaxLength(450);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.EntityType)
            .HasMaxLength(100);

        builder.Property(n => n.EntityId)
            .HasMaxLength(450);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(1000);

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.HasOne(n => n.Tenant)
            .WithMany()
            .HasForeignKey(n => n.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.RecipientUser)
            .WithMany(u => u.ReceivedNotifications)
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.TriggeredByUser)
            .WithMany(u => u.TriggeredNotifications)
            .HasForeignKey(n => n.TriggeredByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => new { n.TenantId, n.RecipientUserId, n.IsRead, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_Tenant_Recipient_IsRead_CreatedAt");

        builder.HasIndex(n => new { n.TenantId, n.RecipientUserId, n.CreatedAt })
            .HasDatabaseName("IX_Notifications_Tenant_Recipient_CreatedAt");

        builder.HasIndex(n => new { n.TenantId, n.EntityType, n.EntityId })
            .HasDatabaseName("IX_Notifications_Tenant_Entity");
    }
}