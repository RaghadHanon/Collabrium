namespace API.Contracts.Roadmaps;

public sealed class RemoveTagsRequest
{
    public required string RoadmapId { get; set; }
    public required string[] TagIds { get; set; }
}
