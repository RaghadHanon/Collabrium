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

        builder.OwnsOne(u => u.ContactDetails, ci =>
        {
            ci.Property(x => x.Email).HasMaxLength(256);
            ci.Property(x => x.PhoneNumber).HasMaxLength(50);

            ci.OwnsOne(x => x.Socials, s =>
            {
                s.Property(p => p.LinkedIn).HasMaxLength(300);
                s.Property(p => p.Facebook).HasMaxLength(300);
                s.Property(p => p.Instagram).HasMaxLength(300);
                s.Property(p => p.YouTube).HasMaxLength(300);
                s.Property(p => p.Github).HasMaxLength(300);
            });
        });

        builder.OwnsOne(u => u.ProfileImage, img =>
        {
            img.Property(x => x.Url).HasMaxLength(1000);
            img.Property(x => x.PublicId).HasMaxLength(300);
        });
        builder.Navigation(u => u.ProfileImage).IsRequired(false);

        builder.HasMany(u => u.Projects)
            .WithOne(p => p.CreatedByUser)
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedWorkItems)
            .WithOne(w => w.AssignedToUser)
            .HasForeignKey(w => w.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.CreatedWorkItems)
            .WithOne(w => w.CreatedByUser)
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.TeamMemberships)
            .WithOne(tm => tm.User)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Skills)
             .WithMany()
             .UsingEntity<Dictionary<string, object>>(
                 "UserSkill",
                 j => j
                     .HasOne<TagEntity>()
                     .WithMany()
                     .HasForeignKey("TagId")
                     .HasConstraintName("FK_UserSkills_Tags_TagId")
                     .OnDelete(DeleteBehavior.Cascade),
                 j => j
                     .HasOne<ApplicationUserEntity>()
                     .WithMany()
                     .HasForeignKey("UserId")
                     .HasConstraintName("FK_UserSkills_ApplicationUsers_UserId")
                     .OnDelete(DeleteBehavior.Cascade),
                 j =>
                 {
                     j.ToTable("UserSkills");

                     j.IndexerProperty<int>("Id")
                         .ValueGeneratedOnAdd();

                     j.HasKey("Id");

                     j.HasIndex("UserId", "TagId")
                         .IsUnique();
                 });

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_ApplicationUser_UserName");
    }
}