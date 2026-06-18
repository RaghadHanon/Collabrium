namespace Application.Features.Posts.DTOs;
public sealed class PostLikeUserDto
{
    public required string UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfileImageUrl { get; set; }

    public DateTime LikedAt { get; set; }
}