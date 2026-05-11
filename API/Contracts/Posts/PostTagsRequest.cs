namespace API.Contracts.Posts
{
    public sealed class PostTagsRequest
    {
        public List<string> TagIds { get; set; } = new();
    }
}
