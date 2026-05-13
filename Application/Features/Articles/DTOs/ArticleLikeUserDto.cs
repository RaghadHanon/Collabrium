namespace Application.Features.Articles.DTOs
{
    public sealed class ArticleLikeUserDto
    {
        public required string UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserProfileImageUrl { get; set; }

        public DateTime LikedAt { get; set; }
    }
}
