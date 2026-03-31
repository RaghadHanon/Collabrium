namespace API.Contracts.RoadmapNodes
{
    public sealed class UpdateTopicNodeRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? Order { get; set; }
    }
}
