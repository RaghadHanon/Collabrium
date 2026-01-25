using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUserEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationUserEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Teams)
            .WithMany(t => t.Members);

        builder.HasMany(u => u.Projects)
            .WithOne(p => p.CreatedByUser)
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ProfileImage)
            .WithOne(i => i.ApplicationUser)
            .HasForeignKey<UserImageEntity>(i => i.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AssignedWorkItems)
            .WithOne(w => w.AssignedToUser)
            .HasForeignKey(w => w.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.UserContactInfo)
            .WithOne(ci => ci.User)
            .HasForeignKey<UserContactInfoEntity>(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_ApplicationUser_UserName");
    }
}