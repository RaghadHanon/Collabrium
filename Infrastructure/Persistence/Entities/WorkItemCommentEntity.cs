namespace Infrastructure.Persistence.Entities;

public class WorkItemCommentEntity : CommentEntity
{
    public string WorkItemId { get; set; } = null!;
    public WorkItemEntity WorkItem { get; set; } = null!;
}