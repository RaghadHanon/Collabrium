using Application.Features.AI.DTOs.Roadmaps;
using Domain.Enums;

namespace Application.Interfaces.AI;
public interface IRoadmapAiDraftService
{
    Task<RoadmapFullDraftDto> GenerateFullDraftAsync(
        RoadmapPath path,
        RoadmapLevel level,
        string goal,
        int topicsCount,
        int subTopicsPerTopic,
        CancellationToken cancellationToken = default);
}