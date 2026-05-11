namespace API.Contracts.Posts
{
    public sealed class UpdatePostRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public Domain.Enums.PostType? Type { get; set; }

        public List<string>? Links { get; set; }
    }
}
