namespace API.Contracts.Articles
{
    public sealed class CreateArticleRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }

        public required string Content { get; set; }

        public bool IsPublished { get; set; } = true;

        public List<string> TagIds { get; set; } = new();
    }
}
