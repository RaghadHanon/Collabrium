using Application.Features.RoadmapNodes.DTOs;

namespace Application.Interfaces.ReadModels;

public interface IRoadmapNodesReadModel
{
    // Tree list
    Task<List<RoadmapNodeDto>> GetByRoadmapIdAsync(
        string tenantId,
        string roadmapId,
        CancellationToken ct = default);

    // Details
    Task<TopicNodeDetailsDto?> GetTopicDetailsAsync(
        string tenantId,
        string roadmapId,
        string topicNodeId,
        CancellationToken ct = default);

    Task<ReferenceNodeDetailsDto?> GetReferenceDetailsAsync(
        string tenantId,
        string roadmapId,
        string referenceNodeId,
        CancellationToken ct = default);
    Task<TopicNodeDto?> GetTopicAsync(
        string tenantId,
        string roadmapId,
        string topicNodeId,
        CancellationToken ct = default);

    Task<ReferenceNodeDto?> GetReferenceAsync(
        string tenantId,
        string roadmapId,
        string referenceNodeId,
        CancellationToken ct = default);

    Task<RoadmapNodeDto?> GetByIdAsync(
        string tenantId,
        string roadmapId,
        string nodeId,
        CancellationToken ct = default);
}
