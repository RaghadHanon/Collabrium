using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class ArticleLikeConfiguration : IEntityTypeConfiguration<ArticleLikeEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleLikeEntity> builder)
        {
            builder.ToTable("ArticleLikes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasMaxLength(50);

            builder.Property(x => x.ArticleId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(x => x.Article)
                .WithMany(a => a.Likes)
                .HasForeignKey(x => x.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ArticleId, x.CreatedByUserId })
                .IsUnique();

            builder.HasIndex(x => new { x.ArticleId, x.CreatedAt });
        }
    }
}
