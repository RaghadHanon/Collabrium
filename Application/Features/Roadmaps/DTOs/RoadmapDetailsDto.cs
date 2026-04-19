namespace Application.Features.Roadmaps.DTOs
{
    public sealed class RoadmapDetailsDto : RoadmapDto
    {
        public IReadOnlyCollection<TopicDto> Topics { get; set; } = Array.Empty<TopicDto>();
    }
}
