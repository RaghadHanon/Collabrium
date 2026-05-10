namespace Application.Features.Posts.DTOs
{
    public sealed class PostImageDto
    {
        public required string Url { get; set; }
        public required string PublicId { get; set; }
    }
}
