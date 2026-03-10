using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IRoadmapNodesRepository
    {
        Task AddTopicAsync(TopicNode topic, CancellationToken ct = default);
        Task AddReferenceAsync(ReferenceNode reference, CancellationToken ct = default);
        Task UpdateAsync(RoadmapNode node, CancellationToken ct = default);

        Task<bool> RemoveAsync(string tenantId, string roadmapId, string nodeId, CancellationToken ct = default);
        public Task<List<RoadmapNode>> GetByRoadmapIdAsync(string tenantId, string roadmapId, CancellationToken ct = default);

        Task<RoadmapNode?> GetByIdAsync(string tenantId, string roadmapId, string nodeId, CancellationToken ct = default);
    }
}
