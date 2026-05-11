namespace API.Contracts.Posts
{
    public sealed class GetPostsRequest
    {
        public string? Search { get; set; }
        public string? CreatedByUserId { get; set; }

        public Domain.Enums.PostType? Type { get; set; }
        public bool? IsPinned { get; set; }

        public List<string>? TagIdsAny { get; set; }
        public List<string>? TagIdsAll { get; set; }

        public bool? HasImages { get; set; }
        public bool? HasLinks { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }
        public DateTime? UpdatedBefore { get; set; }

        public int PageSize { get; set; } = 10;
        public DateTime? Cursor { get; set; }
    }
}
