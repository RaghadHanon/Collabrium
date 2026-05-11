using Domain.Enums;

namespace API.Contracts.Posts
{
    public sealed class CreatePostRequest
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required PostType Type { get; set; }

        public List<string> TagIds { get; set; } = new();
        public List<string> Links { get; set; } = new();
    }
}
