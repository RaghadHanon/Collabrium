using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class LikeConfiguration : IEntityTypeConfiguration<LikeEntity>
    {
        public void Configure(EntityTypeBuilder<LikeEntity> builder)
        {
            builder.ToTable("Likes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasDiscriminator<string>("LikeType")
                .HasValue<PostLikeEntity>("Post")
                .HasValue<ArticleLikeEntity>("Article");
        }
    }
}
