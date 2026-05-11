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

            builder.Property(x => x.ArticleId)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(x => x.Article)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
