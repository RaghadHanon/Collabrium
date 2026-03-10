using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IRoadmapResourcesRepository
    {
        Task AddAsync(
            RoadmapResource resource,
            CancellationToken ct = default);

        Task UpdateAsync(
            RoadmapResource resource,
            CancellationToken ct = default);

        Task<bool> RemoveAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId,
            CancellationToken ct = default);

        Task<RoadmapResource?> GetByIdAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            string resourceId,
            CancellationToken ct = default);

        Task<IReadOnlyList<RoadmapResource>> GetBySubTopicIdAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            CancellationToken ct = default);
    }
}
