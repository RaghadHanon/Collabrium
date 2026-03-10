using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProjectHistoryConfiguration : IEntityTypeConfiguration<ProjectHistoryEntity>
{
    public void Configure(EntityTypeBuilder<ProjectHistoryEntity> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(x => new { x.ProjectId, x.CreatedAt });

        builder.Property(x => x.PropertyName)
            .HasMaxLength(200);

        builder.Property(x => x.Scope)
            .IsRequired();

        builder.HasOne(x => x.Project)
            .WithMany(p => p.ProjectHistories)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MadeByUser)
           .WithMany(u => u.ProjectsHistories)
           .HasForeignKey(x => x.MadeByUserId)
           .OnDelete(DeleteBehavior.Restrict);
    }
}