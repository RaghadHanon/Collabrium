using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class ArticleLikeConfiguration : IEntityTypeConfiguration<ArticleLikeEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleLikeEntity> builder)
        {
            builder.Property(x => x.ArticleId)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(x => x.Article)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ArticleId, x.CreatedByUserId })
                .IsUnique();

            builder.HasIndex(x => new { x.ArticleId, x.CreatedAt });
        }
    }
}
