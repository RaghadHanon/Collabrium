using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItemEntity>
{
    public void Configure(EntityTypeBuilder<WorkItemEntity> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
               .IsRequired()
               .HasMaxLength(250);

        builder.HasOne(w => w.Project)
               .WithMany(p => p.WorkItems)
               .HasForeignKey(w => w.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Sprint)
               .WithMany(s => s.WorkItems)
               .HasForeignKey(w => w.SprintId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(w => w.AssignedToUser)
               .WithMany(u => u.AssignedWorkItems)
               .HasForeignKey(w => w.AssignedToUserId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.CreatedByUser)
               .WithMany(u => u.CreatedWorkItems)
               .HasForeignKey(w => w.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.OutgoingRelations)
               .WithOne(r => r.OutgoingWorkItem)
               .HasForeignKey(r => r.OutgoingWorkItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.IncomingRelations)
               .WithOne(r => r.IncomingWorkItem)
               .HasForeignKey(r => r.IncomingWorkItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Comments)
               .WithOne(c => c.WorkItem)
               .HasForeignKey(c => c.WorkItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder
             .HasDiscriminator<WorkItemType>("WorkItemType")
             .HasValue<WorkItemEntity>(WorkItemType.WorkItem) // base type (default)
             .HasValue<EpicEntity>(WorkItemType.Epic)
             .HasValue<UserStoryEntity>(WorkItemType.UserStory)
             .HasValue<BugEntity>(WorkItemType.Bug)
             .HasValue<TestCaseEntity>(WorkItemType.TestCase)
             .HasValue<TaskItemEntity>(WorkItemType.TaskItem);
    }
}
