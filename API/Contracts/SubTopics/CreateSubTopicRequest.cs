namespace API.Contracts.SubTopics
{
    public sealed class CreateSubTopicRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}
