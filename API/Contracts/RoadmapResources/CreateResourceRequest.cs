using Domain.Enums;

namespace API.Contracts.RoadmapResources
{
    public sealed class CreateResourceRequest
    {
        public required string Title { get; init; }
        public required string Url { get; init; }

        public RoadmapResourceType Type { get; init; }
        public int Order { get; init; }
    }
}
