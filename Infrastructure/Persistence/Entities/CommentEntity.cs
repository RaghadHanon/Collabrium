namespace Infrastructure.Persistence.Entities;

public class CommentEntity : DbEntity
{
    public string Content { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public string WorkItemId { get; set; } = null!;

    public ApplicationUserEntity CreatedByUser { get; set; } = null!;
    public WorkItemEntity WorkItem { get; set; } = null!;
}
