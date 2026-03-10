namespace API.Contracts.SubTopics
{
    public sealed class UpdateSubTopicRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required int Order { get; set; }
    }
}
