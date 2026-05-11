namespace API.Contracts.Posts
{
    public sealed class GetPostCommentsRequest
    {
        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }
}
