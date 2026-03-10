using Domain.Enums;

namespace Application.Features.Roadmaps.DTOs
{
    public sealed class RoadmapDetailsDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public RoadmapPath Path { get; set; }
        public RoadmapLevel Level { get; set; }
        public IReadOnlyCollection<TopicDto> Topics { get; set; } = Array.Empty<TopicDto>();


    }
}
