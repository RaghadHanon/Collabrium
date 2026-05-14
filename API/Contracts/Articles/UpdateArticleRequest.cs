namespace API.Contracts.Articles
{
    public sealed class UpdateArticleRequest
    {
        public string? Title { get; set; }

        public string? Content { get; set; }
        public string? Description { get; set; }
        public bool? IsPublished { get; set; }
    }
}
