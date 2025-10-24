namespace Domain.Entities;

public class Comment : Entity
{
    public string Content { get; set; } = null!;
    public string CreatedByUserId { get; set; } = null!;
    public required string WorkItemId { get; set; }

    // Navigation
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public WorkItem WorkItem { get; set; } = null!;
}