using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<AttachmentEntity>
{
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(c => c.UploadedByUser)
            .WithMany()
            .HasForeignKey(c => c.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}