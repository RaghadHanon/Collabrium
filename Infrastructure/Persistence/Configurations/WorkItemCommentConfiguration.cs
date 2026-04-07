using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WorkItemCommentConfiguration : IEntityTypeConfiguration<WorkItemCommentEntity>
{
    public void Configure(EntityTypeBuilder<WorkItemCommentEntity> builder)
    {
        builder.ToTable("WorkItemComments");

        builder.HasOne(c => c.WorkItem)
            .WithMany(w => w.Comments)
            .HasForeignKey(c => c.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

