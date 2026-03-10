using Domain.Enums;

namespace Application.Features.RoadmapResources.DTOs
{
    public sealed class ResourceDto
    {
        public required string Id { get; set; }
        public required string SubTopicId { get; set; }

        public required string Title { get; set; }
        public required string Url { get; set; }

        public RoadmapResourceType Type { get; set; }
        public int Order { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
