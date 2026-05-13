namespace Application.Features.RoadmapNodes.DTOs
{
    public sealed class ReferenceNodeDto : RoadmapNodeDto
    {
        public required string RefRoadmapId { get; set; }
        public string? RefRoadmapTitle { get; set; }
        public bool? RefRoadmapIsVisible { get; set; }
    }
}
