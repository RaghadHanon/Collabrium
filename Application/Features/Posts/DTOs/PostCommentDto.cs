namespace Application.Features.Posts.DTOs
{
    public sealed class PostCommentDto
    {
        public required string Id { get; set; }

        public required string PostId { get; set; }

        public required string CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CreatedByUserProfileImageUrl { get; set; }

        public required string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
