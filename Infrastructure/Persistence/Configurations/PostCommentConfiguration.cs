using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public sealed class PostCommentConfiguration : IEntityTypeConfiguration<PostCommentEntity>
    {
        public void Configure(EntityTypeBuilder<PostCommentEntity> builder)
        {
            builder.Property(x => x.PostId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(x => x.Post)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.PostId);
        }
    }
}
