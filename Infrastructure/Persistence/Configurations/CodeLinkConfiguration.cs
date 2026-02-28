using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CodeLinkConfiguration : IEntityTypeConfiguration<CodeLinkEntity>
{
    public void Configure(EntityTypeBuilder<CodeLinkEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}