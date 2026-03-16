namespace API.Contracts.SubTopics
{
    public sealed class UpdateSubTopicRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Order { get; set; }
    }
}
