using Domain.Enums;

namespace Application.Features.SubTopics.DTOs
{
    public sealed class RoadmapResourceDto
    {
        public required string Id { get; set; }
        public required string SubTopicId { get; set; }

        public required string Title { get; set; }
        public required string Url { get; set; }

        public RoadmapResourceType Type { get; set; }
        public int Order { get; set; }
    }
}
