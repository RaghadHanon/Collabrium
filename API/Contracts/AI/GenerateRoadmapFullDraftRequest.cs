using Domain.Enums;

namespace API.Contracts.AI;

public sealed class GenerateRoadmapFullDraftRequest
{
    public RoadmapPath Path { get; set; }
    public RoadmapLevel Level { get; set; }

    public string Goal { get; set; } = string.Empty;

    public int TopicsCount { get; set; } = 5;
    public int SubTopicsPerTopic { get; set; } = 3;
    public int ResourcesPerSubTopic { get; set; } = 2;
}