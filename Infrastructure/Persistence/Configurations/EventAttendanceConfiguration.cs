using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;
public sealed class EventAttendanceConfiguration : IEntityTypeConfiguration<EventAttendanceEntity>
{
    public void Configure(EntityTypeBuilder<EventAttendanceEntity> builder)
    {
        builder.ToTable("EventAttendances");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.EventId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(a => a.Tenant)
            .WithMany()
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Event)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.EventId, a.UserId })
            .IsUnique();

        builder.HasIndex(a => new { a.TenantId, a.EventId });

        builder.HasIndex(a => new { a.TenantId, a.UserId });
    }
}