using Application.Features.Tags.DTOs;

namespace Application.Features.Articles.DTOs
{
    public sealed class ArticleDetailsDto
    {
        public required string Id { get; set; }

        public required string Title { get; set; }
        public required string Content { get; set; }

        public string? CoverImageUrl { get; set; }
        public string? CoverImagePublicId { get; set; }

        public bool IsPublished { get; set; }

        public required string CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string? CreatedByUserProfileImageUrl { get; set; }

        public List<TagDto> Tags { get; set; } = new();

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        public bool IsLikedByCurrentUser { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
