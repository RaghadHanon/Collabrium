using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class ArticleConfiguration : IEntityTypeConfiguration<ArticleEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleEntity> builder)
        {
            builder.ToTable("Articles");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasMaxLength(50);
            builder.Property(a => a.TenantId).IsRequired().HasMaxLength(450);
            builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Content).IsRequired();
            builder.Property(a => a.CreatedByUserId).IsRequired().HasMaxLength(450);
            builder.Property(a => a.IsPublished).IsRequired();
            builder.Property(a => a.Description)
            .HasMaxLength(1000);
            builder.HasOne(a => a.Tenant)
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(a => a.Tags)
                .WithMany()
                .UsingEntity(j => j.ToTable("ArticleTags"));
            builder.OwnsOne(a => a.CoverImage, image =>
            {
                image.Property(i => i.Url)
                    .HasColumnName("CoverImageUrl")
                    .HasMaxLength(2000);

                image.Property(i => i.PublicId)
                    .HasColumnName("CoverImagePublicId")
                    .HasMaxLength(500);
            });
            builder.HasIndex(a => new { a.TenantId, a.CreatedAt });
            builder.HasIndex(a => new { a.TenantId, a.IsPublished });
            builder.HasIndex(a => new { a.TenantId, a.Title });
        }
    }
}
