namespace API.Contracts.Roadmaps
{
    public sealed class ToggleRoadmapVisibleRequest
    {
        public required string RoadmapId { get; set; }
        public required bool IsVisible { get; set; }
    }
}
