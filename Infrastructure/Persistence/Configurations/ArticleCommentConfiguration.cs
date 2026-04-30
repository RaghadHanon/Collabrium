using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class ArticleCommentConfiguration : IEntityTypeConfiguration<ArticleCommentEntity>
    {
        public void Configure(EntityTypeBuilder<ArticleCommentEntity> builder)
        {
            builder.ToTable("ArticleComments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasMaxLength(50);

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

            builder.HasIndex(c => new { c.ArticleId, c.CreatedAt });
        }
    }
}
