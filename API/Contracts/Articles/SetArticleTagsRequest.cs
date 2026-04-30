namespace API.Contracts.Articles
{
    public sealed class SetArticleTagsRequest
    {
        public List<string> TagIds { get; set; } = new();
    }
}
