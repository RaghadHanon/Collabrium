namespace Application.Features.Posts.DTOs;
public sealed class PostsCursorDto
{
    public bool IsPinned { get; set; }
    public int EngagementScore { get; set; }
    public DateTime CreatedAt { get; set; }
}