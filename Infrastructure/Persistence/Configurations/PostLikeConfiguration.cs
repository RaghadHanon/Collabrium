using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class PostLikeConfiguration : IEntityTypeConfiguration<PostLikeEntity>
    {
        public void Configure(EntityTypeBuilder<PostLikeEntity> builder)
        {
            builder.Property(x => x.PostId)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(x => x.Post)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.PostId, x.CreatedByUserId })
                .IsUnique();

            builder.HasIndex(x => new { x.PostId, x.CreatedAt });
        }
    }
}
