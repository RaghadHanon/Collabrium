using Domain.Enums;

namespace API.Contracts.Roadmaps
{
    public sealed class UpdateRoadmapRequest
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public RoadmapPath? Path { get; init; }
        public RoadmapLevel? Level { get; init; }
        public DateTime? PublishedAt { get; init; }
    }
}
