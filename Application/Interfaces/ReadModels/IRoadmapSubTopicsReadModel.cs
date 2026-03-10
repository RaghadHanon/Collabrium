using Application.Features.Roadmaps.DTOs;
using Application.Features.SubTopics.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface IRoadmapSubTopicsReadModel
    {
        Task<List<SubTopicDto>> GetByTopicAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            CancellationToken cancellationToken = default);
        Task<SubTopicDetailsDto?> GetDetailsAsync(
          string tenantId,
          string roadmapId,
          string topicNodeId,
          string subTopicId,
          CancellationToken ct = default);

        Task<bool> TopicExistsInRoadmapAsync(
          string tenantId,
          string roadmapId,
          string topicNodeId,
          CancellationToken ct = default);

        Task<SubTopicDto?> GetByIdAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            CancellationToken ct = default);
    }
}
