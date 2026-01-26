using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Domain)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Name).IsUnique().HasDatabaseName("IX_Tenant_Name");
        builder.HasIndex(t => t.Domain).IsUnique().HasDatabaseName("IX_Tenant_Domain");

        builder.OwnsOne(t => t.ContactInfo, ci =>
        {
            ci.Property(x => x.Email).HasMaxLength(256);
            ci.Property(x => x.PhoneNumber).HasMaxLength(50);

            ci.OwnsOne(x => x.Socials, s =>
            {
                s.Property(p => p.LinkedIn).HasMaxLength(300);
                s.Property(p => p.Facebook).HasMaxLength(300);
                s.Property(p => p.Instagram).HasMaxLength(300);
                s.Property(p => p.YouTube).HasMaxLength(300);
            });
        });

        builder.OwnsOne(t => t.ProfileImage, img =>
        {
            img.Property(x => x.Url).HasMaxLength(1000);
            img.Property(x => x.PublicId).HasMaxLength(300);
        });
        builder.Navigation(t => t.ProfileImage).IsRequired(false);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Teams)
            .WithOne(tm => tm.Tenant)
            .HasForeignKey(tm => tm.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Projects)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}