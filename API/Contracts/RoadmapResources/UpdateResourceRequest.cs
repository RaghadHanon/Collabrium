using Domain.Enums;

namespace API.Contracts.RoadmapResources
{
    public sealed class UpdateResourceRequest
    {
        public string? Title { get; init; }
        public string? Url { get; init; }

        public RoadmapResourceType? Type { get; init; }
    }
}
