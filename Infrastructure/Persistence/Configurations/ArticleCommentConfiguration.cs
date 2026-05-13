using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class ArticleCommentConfiguration : IEntityTypeConfiguration<ArticleCommentEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleCommentEntity> builder)
        {
            builder.ToTable("ArticleComments");

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.ArticleId)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Article)
                .WithMany(a => a.Comments)
                .HasForeignKey(c => c.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
