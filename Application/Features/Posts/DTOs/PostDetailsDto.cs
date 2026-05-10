namespace Application.Features.Posts.DTOs
{
    public sealed class PostDetailsDto : PostDto
    {
        public List<PostCommentDto> Comments { get; set; } = new();
    }
}
