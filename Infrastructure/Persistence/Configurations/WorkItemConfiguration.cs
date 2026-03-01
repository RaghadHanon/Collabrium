using Domain.Enums;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

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

        builder.HasMany(w => w.Relations)
            .WithOne(r => r.SourceWorkItem)
            .HasForeignKey(r => r.SourceWorkItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Comments)
            .WithOne(c => c.WorkItem)
            .HasForeignKey(c => c.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.CodeLinks)
            .WithOne(cl => cl.WorkItem)
            .HasForeignKey(cl => cl.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Attachments)
            .WithOne(a => a.WorkItem)
            .HasForeignKey(a => a.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.ProjectId, w.SprintId, w.CreatedAt });
        builder.HasIndex(w => new { w.ProjectId, w.Type });
        builder.HasIndex(w => new { w.ProjectId, w.Status });
        builder.HasIndex(w => new { w.ProjectId, w.AssignedToUserId });
        builder.HasIndex(w => new { w.ProjectId, w.Priority });
        builder.HasIndex(w => new { w.ProjectId, w.CreatedAt });

        builder
            .HasDiscriminator(w => w.Type)
            .HasValue<WorkItemEntity>(WorkItemType.WorkItem)
            .HasValue<EpicEntity>(WorkItemType.Epic)
            .HasValue<UserStoryEntity>(WorkItemType.UserStory)
            .HasValue<TaskItemEntity>(WorkItemType.TaskItem)
            .HasValue<BugEntity>(WorkItemType.Bug)
            .HasValue<TestCaseEntity>(WorkItemType.TestCase);
    }
}