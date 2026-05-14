namespace API.Contracts.Articles
{
    public sealed class RemoveArticleTagsRequest
    {
        public List<string> TagIds { get; set; } = new();
    }
}
