namespace API.Contracts.Tags
{
    public sealed class GetTagsRequest
    {
        public string? NameContains { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }

        public DateTime? Cursor { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
