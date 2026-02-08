using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WorkItemRelationConfiguration : IEntityTypeConfiguration<WorkItemRelationEntity>
{
    public void Configure(EntityTypeBuilder<WorkItemRelationEntity> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.SourceWorkItem)
            .WithMany(w => w.Relations)
            .HasForeignKey(r => r.SourceWorkItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.SourceWorkItemId, r.TargetWorkItemId, r.RelationType })
            .IsUnique()
            .HasDatabaseName("UX_WorkItemRelation_Unique");
    }
}