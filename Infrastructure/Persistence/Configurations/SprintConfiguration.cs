using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SprintConfiguration : IEntityTypeConfiguration<SprintEntity>
{
    public void Configure(EntityTypeBuilder<SprintEntity> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(s => s.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.WorkItems)
            .WithOne(w => w.Sprint)
            .HasForeignKey(w => w.SprintId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => s.Code).IsUnique();
    }
}