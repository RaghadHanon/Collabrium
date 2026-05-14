namespace API.Contracts.Articles
{
    public sealed class GetArticlesRequest
    {
        public string? Search { get; set; }

        public bool? IsPublished { get; set; }

        public string? CreatedByUserId { get; set; }

        public List<string>? TagIdsAny { get; set; }

        public List<string>? TagIdsAll { get; set; }

        public DateTime? CreatedAfter { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }

        public DateTime? UpdatedBefore { get; set; }

        public int PageSize { get; set; } = 10;

        public DateTime? Cursor { get; set; }
    }
}
