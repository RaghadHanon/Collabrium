namespace Application.Features.Articles.DTOs
{
    public sealed class ArticleCommentDto
    {
        public required string Id { get; set; }

        public required string ArticleId { get; set; }
        public required string Content { get; set; }

        public required string CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string? CreatedByUserProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
