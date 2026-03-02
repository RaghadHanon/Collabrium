namespace API.Contracts.RoadmapNodes
{
    public sealed class CreateTopicNodeRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required int Order { get; set; }
    }
}
