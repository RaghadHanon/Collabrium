using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class RoadmapResourcesRepository : IRoadmapResourcesRepository
{
    private readonly AppDbContext _dbContext;

    public RoadmapResourcesRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(RoadmapResource resource, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var entity = RoadmapResourcesMapper.ToEntity(resource);
        await _dbContext.Resources.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(RoadmapResource resource, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var entity = await _dbContext.Resources
            .FirstOrDefaultAsync(r => r.Id == resource.Id, ct);

        if (entity is null)
            throw new InvalidOperationException("Resource not found.");

        RoadmapResourcesMapper.UpdateEntity(resource, entity);
    }

    public async Task<bool> RemoveAsync(
        string tenantId,
        string roadmapId,
        string topicNodeId,
        string subTopicId,
        string resourceId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(roadmapId) ||
            string.IsNullOrWhiteSpace(topicNodeId) ||
            string.IsNullOrWhiteSpace(subTopicId) ||
            string.IsNullOrWhiteSpace(resourceId))
            return false;

        var tId = tenantId.Trim();
        var rId = roadmapId.Trim();
        var tNodeId = topicNodeId.Trim();
        var sId = subTopicId.Trim();
        var resId = resourceId.Trim();

        var entity = await _dbContext.Resources
            .Include(x => x.SubTopic)
            .ThenInclude(st => st.TopicNode)
            .FirstOrDefaultAsync(x =>
                x.Id == resId &&
                x.SubTopicId == sId &&
                x.SubTopic.TopicNodeId == tNodeId &&
                x.SubTopic.TopicNode.RoadmapId == rId &&
                x.SubTopic.TopicNode.TenantId == tId,
                ct);

        if (entity is null)
            return false;

        _dbContext.Resources.Remove(entity);
        return true;
    }

    public async Task<RoadmapResource?> GetByIdAsync(
        string tenantId,
        string roadmapId,
        string topicNodeId,
        string subTopicId,
        string resourceId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(roadmapId) ||
            string.IsNullOrWhiteSpace(topicNodeId) ||
            string.IsNullOrWhiteSpace(subTopicId) ||
            string.IsNullOrWhiteSpace(resourceId))
            return null;

        var tId = tenantId.Trim();
        var rId = roadmapId.Trim();
        var tNodeId = topicNodeId.Trim();
        var sId = subTopicId.Trim();
        var resId = resourceId.Trim();

        var entity = await _dbContext.Resources
            .AsNoTracking()
            .Include(x => x.SubTopic)
            .ThenInclude(st => st.TopicNode)
            .FirstOrDefaultAsync(x =>
                x.Id == resId &&
                x.SubTopicId == sId &&
                x.SubTopic.TopicNodeId == tNodeId &&
                x.SubTopic.TopicNode.RoadmapId == rId &&
                x.SubTopic.TopicNode.TenantId == tId,
                ct);

        return entity is null
            ? null
            : RoadmapResourcesMapper.ToDomain(entity);
    }

    public async Task<List<RoadmapResource>> GetBySubTopicIdAsync(
        string subTopicId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(subTopicId))
            return new();

        var sId = subTopicId.Trim();

        var entities = await _dbContext.Resources
            .AsNoTracking()
            .Where(x => x.SubTopicId == sId)
            .OrderBy(x => x.Order)
            .ToListAsync(ct);

        return entities.Select(RoadmapResourcesMapper.ToDomain).ToList();
    }

    public async Task<int> GetMaxOrderAsync(
        string subTopicId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(subTopicId))
            return 0;

        var sId = subTopicId.Trim();

        var max = await _dbContext.Resources
            .Where(r => r.SubTopicId == sId)
            .Select(r => (int?) r.Order)
            .MaxAsync(ct);

        return max ?? 0;
    }
    public async Task SwapOrderAsync(
string subTopicId,
string firstResourceId,
string secondResourceId,
CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(subTopicId) ||
            string.IsNullOrWhiteSpace(firstResourceId) ||
            string.IsNullOrWhiteSpace(secondResourceId))
        {
            throw new ArgumentException("SubTopicId and resource ids are required.");
        }

        var sId = subTopicId.Trim();
        var firstId = firstResourceId.Trim();
        var secondId = secondResourceId.Trim();

        if (string.Equals(firstId, secondId, StringComparison.Ordinal))
            return;

        var first = await _dbContext.Resources
            .FirstOrDefaultAsync(r => r.Id == firstId && r.SubTopicId == sId, ct);

        var second = await _dbContext.Resources
            .FirstOrDefaultAsync(r => r.Id == secondId && r.SubTopicId == sId, ct);

        if (first is null || second is null)
            throw new InvalidOperationException("One or both resources were not found.");

        var firstOrder = first.Order;
        var secondOrder = second.Order;

        first.Order = 0;
        await _dbContext.SaveChangesAsync(ct);

        second.Order = firstOrder;
        await _dbContext.SaveChangesAsync(ct);

        first.Order = secondOrder;
        await _dbContext.SaveChangesAsync(ct);
    }
}
