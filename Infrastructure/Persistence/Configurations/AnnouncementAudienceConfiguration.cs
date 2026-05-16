using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;
public sealed class AnnouncementAudienceConfiguration : IEntityTypeConfiguration<AnnouncementAudienceEntity>
{
    public void Configure(EntityTypeBuilder<AnnouncementAudienceEntity> builder)
    {
        builder.ToTable("AnnouncementAudiences");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AnnouncementId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.TeamId)
            .HasMaxLength(450);

        builder.Property(a => a.RoleName)
            .HasMaxLength(100);

        builder.Property(a => a.UserId)
            .HasMaxLength(450);

        builder.HasOne(a => a.Team)
            .WithMany()
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.AnnouncementId, a.Type });

        builder.HasIndex(a => a.TeamId);

        builder.HasIndex(a => a.RoleName);

        builder.HasIndex(a => a.UserId);


    }
}