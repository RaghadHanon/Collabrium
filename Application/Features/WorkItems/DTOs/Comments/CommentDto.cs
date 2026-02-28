namespace Application.Features.WorkItems.DTOs.Comments;

public class CommentDto
{
    public required string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public required string Content { get; set; }
    public required string CreatedByUserId { get; set; }
}