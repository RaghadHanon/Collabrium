namespace API.Contracts.Articles
{
    public sealed class GetArticleCommentsRequest
    {
        public int PageSize { get; set; } = 10;

        public DateTime? Cursor { get; set; }
    }
}
