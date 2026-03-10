using Domain.Enums;

namespace API.Contracts.Roadmaps
{
    public sealed class CreateRoadmapRequest
    {
        public required string Title { get; init; }
        public string? Description { get; init; }
        public RoadmapPath Path { get; init; }
        public RoadmapLevel Level { get; init; }
    }
}
