using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SprintConfiguration : IEntityTypeConfiguration<Sprint>
{
    public void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
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
    }
}
