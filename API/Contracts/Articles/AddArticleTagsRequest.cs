namespace API.Contracts.Articles
{
    public sealed class AddArticleTagsRequest
    {
        public List<string> TagIds { get; set; } = new();
    }
}
