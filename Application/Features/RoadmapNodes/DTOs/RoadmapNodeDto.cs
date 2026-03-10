namespace Application.Features.RoadmapNodes.DTOs
{
    public abstract class RoadmapNodeDto
    {
        public required string Id { get; set; }
        public required string RoadmapId { get; set; }

        public required string Title { get; set; }
        public string? Description { get; set; }

        public int Order { get; set; }

        public required string NodeType { get; set; } // "Topic" | "Reference"

        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

}
