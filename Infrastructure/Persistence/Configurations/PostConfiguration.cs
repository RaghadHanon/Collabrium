using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class PostConfiguration : IEntityTypeConfiguration<PostEntity>
    {
        public void Configure(EntityTypeBuilder<PostEntity> builder)
        {
            builder.ToTable("Posts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(5000);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.IsPinned)
                .IsRequired();

            builder.Property(x => x.PinnedByUserId)
                .HasMaxLength(450);

            builder.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PinnedByUser)
                .WithMany()
                .HasForeignKey(x => x.PinnedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Tags)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "PostTags",
                    right => right
                        .HasOne<TagEntity>()
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<PostEntity>()
                        .WithMany()
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("PostTags");

                        join.HasKey("PostId", "TagId");

                        join.Property<string>("PostId")
                            .HasMaxLength(450);

                        join.Property<string>("TagId")
                            .HasMaxLength(450);

                        join.HasIndex("TagId");
                    });

            builder.OwnsMany(x => x.Links, links =>
            {
                links.ToTable("PostLinks");

                links.WithOwner()
                    .HasForeignKey("PostId");

                links.Property<int>("Id");
                links.HasKey("Id");

                links.Property<string>("PostId")
                    .HasMaxLength(450);

                links.Property(x => x.Url)
                    .IsRequired()
                    .HasMaxLength(1000);
            });

            builder.OwnsMany(x => x.Images, images =>
            {
                images.ToTable("PostImages");

                images.WithOwner()
                    .HasForeignKey("PostId");

                images.Property<int>("Id");
                images.HasKey("Id");

                images.Property<string>("PostId")
                    .HasMaxLength(450);

                images.Property(x => x.Url)
                    .IsRequired()
                    .HasMaxLength(1000);

                images.Property(x => x.PublicId)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
            builder.HasIndex(x => new { x.TenantId, x.IsPinned });
            builder.HasIndex(x => x.CreatedByUserId);
        }
    }
}

