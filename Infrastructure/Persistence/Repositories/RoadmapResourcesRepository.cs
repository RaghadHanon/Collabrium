using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
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
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId) || string.IsNullOrWhiteSpace(resourceId))
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
                    x.Id == resId
                    && x.SubTopicId == sId
                    && x.SubTopic.TopicNodeId == tNodeId
                    && x.SubTopic.TopicNode.RoadmapId == rId
                    && x.SubTopic.TopicNode.TenantId == tId,
                    ct);

            if (entity is null) return false;

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
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId) || string.IsNullOrWhiteSpace(resourceId))
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
                    x.Id == resId
                    && x.SubTopicId == sId
                    && x.SubTopic.TopicNodeId == tNodeId
                    && x.SubTopic.TopicNode.RoadmapId == rId
                    && x.SubTopic.TopicNode.TenantId == tId,
                    ct);

            return entity is null ? null : RoadmapResourcesMapper.ToDomain(entity);
        }

        public async Task<IReadOnlyList<RoadmapResource>> GetBySubTopicIdAsync(
            string tenantId,
            string roadmapId,
            string topicNodeId,
            string subTopicId,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId) || string.IsNullOrWhiteSpace(topicNodeId) || string.IsNullOrWhiteSpace(subTopicId))
                return new List<RoadmapResource>();
            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();
            var tNodeId = topicNodeId.Trim();
            var sId = subTopicId.Trim();

            var entities = await _dbContext.Resources
                .AsNoTracking()
                .Include(x => x.SubTopic)
                .ThenInclude(st => st.TopicNode)
                .Where(x =>
                    x.SubTopicId == sId
                    && x.SubTopic.TopicNodeId == tNodeId
                    && x.SubTopic.TopicNode.RoadmapId == rId
                    && x.SubTopic.TopicNode.TenantId == tId)
                .OrderBy(x => x.Order)
                .ToListAsync(ct);

            return entities.Select(RoadmapResourcesMapper.ToDomain).ToList();
        }


    }
}
