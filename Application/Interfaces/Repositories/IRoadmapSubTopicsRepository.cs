using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IRoadmapSubTopicsRepository
{
    Task AddAsync(RoadmapSubTopic subTopic, CancellationToken ct = default);

    Task UpdateAsync(RoadmapSubTopic subTopic, CancellationToken ct = default);

    Task<RoadmapSubTopic?> GetByIdAsync(
        string topicNodeId,
        string subTopicId,
        CancellationToken ct = default);

    Task<List<RoadmapSubTopic>> GetByTopicIdAsync(
        string topicNodeId,
        CancellationToken ct = default);

    Task<int> GetMaxOrderAsync(
        string topicNodeId,
        CancellationToken ct = default);

    Task<bool> RemoveAsync(
        string topicNodeId,
        string subTopicId,
        CancellationToken ct = default);

    Task<bool> HasResourcesAsync(
        string subTopicId,
        CancellationToken ct = default);
    Task SwapOrderAsync(
        string topicNodeId,
        string firstSubTopicId,
        string secondSubTopicId,
        CancellationToken ct = default);
}
