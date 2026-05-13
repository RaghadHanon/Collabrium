using Application.Features.RoadmapResources.DTOs;

namespace Application.Interfaces.ReadModels;

public interface IRoadmapResourcesReadModel
{
    Task<ResourceDto?> GetByIdAsync(
        string tenantId,
        string roadmapId,
        string topicNodeId,
        string subTopicId,
        string resourceId,
        CancellationToken ct = default);

    Task<List<ResourceDto>> GetBySubTopicIdAsync(
        string tenantId,
        string roadmapId,
        string topicNodeId,
        string subTopicId,
        CancellationToken ct = default);
}
