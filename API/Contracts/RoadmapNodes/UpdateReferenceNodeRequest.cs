namespace API.Contracts.RoadmapNodes
{
    public sealed class UpdateReferenceNodeRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required int Order { get; set; }
        public required string RefRoadmapId { get; set; }
    }
}
