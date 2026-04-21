namespace API.Contracts.RoadmapNodes
{
    public sealed class UpdateReferenceNodeRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? RefRoadmapId { get; set; }
    }
}
