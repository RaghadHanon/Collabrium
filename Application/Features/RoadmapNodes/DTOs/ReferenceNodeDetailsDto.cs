namespace Application.Features.RoadmapNodes.DTOs
{
    public sealed class ReferenceNodeDetailsDto
    {
        public required string Id { get; set; }
        public required string RoadmapId { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }

        public int Order { get; set; }
        public required string RefRoadmapId { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
