namespace API.Contracts.RoadmapNodes
{
    public sealed class CreateReferenceNodeRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string RefRoadmapId { get; set; }
    }
}
