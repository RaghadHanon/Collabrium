using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WorkItemRelationConfiguration : IEntityTypeConfiguration<WorkItemRelationEntity>
{
    public void Configure(EntityTypeBuilder<WorkItemRelationEntity> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.OutgoingWorkItem)
               .WithMany(w => w.OutgoingRelations)
               .HasForeignKey(r => r.OutgoingWorkItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.IncomingWorkItem)
               .WithMany(w => w.IncomingRelations)
               .HasForeignKey(r => r.IncomingWorkItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
