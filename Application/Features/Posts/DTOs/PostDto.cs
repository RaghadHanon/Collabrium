using Application.Features.Tags.DTOs;
using Application.Features.Users.DTOs;

namespace Application.Features.Posts.DTOs;

public class PostDto
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }

    public required UserDto CreatedByUser { get; set; }

    public required string Title { get; set; }
    public required string Content { get; set; }

    public Domain.Enums.PostType Type { get; set; }

    public bool IsPinned { get; set; }
    public string? PinnedByUserId { get; set; }
    public string? PinnedByUserName { get; set; }
    public DateTime? PinnedAt { get; set; }

    public List<TagDto> Tags { get; set; } = new();
    public List<string> Links { get; set; } = new();
    public List<PostImageDto> Images { get; set; } = new();

    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
