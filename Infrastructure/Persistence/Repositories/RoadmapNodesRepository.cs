using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class RoadmapNodesRepository : IRoadmapNodesRepository
    {
        private readonly AppDbContext _dbContext;
        public RoadmapNodesRepository(AppDbContext dbContext)
            => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        public async Task AddTopicAsync(TopicNode topic, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(topic);
            var entity = (TopicNodeEntity)RoadmapNodeMapper.ToEntity(topic);
            await _dbContext.Set<TopicNodeEntity>().AddAsync(entity, ct);
        }

        public async Task AddReferenceAsync(ReferenceNode reference, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(reference);
            var entity = (ReferenceNodeEntity)RoadmapNodeMapper.ToEntity(reference);
            await _dbContext.Set<ReferenceNodeEntity>().AddAsync(entity, ct);
        }

        public async Task<bool> RemoveAsync(string tenantId, string roadmapId, string nodeId, CancellationToken ct = default)
        {
            if (!TryNormalize(tenantId, roadmapId, nodeId, out var tId, out var rId, out var nId))
                return false;

            var entity = await _dbContext.Set<RoadmapNodeEntity>()
          .Include(n => n.Roadmap)
          .FirstOrDefaultAsync(n =>
              n.Id == nId &&
              n.RoadmapId == rId &&
              n.Roadmap.TenantId == tId, ct);

            if (entity is null) return false;

            _dbContext.Remove(entity);
            return true;
        }

        public async Task UpdateAsync(RoadmapNode node, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(node);
            var entity = await _dbContext.Set<RoadmapNodeEntity>()
                .FirstOrDefaultAsync(n => n.Id == node.Id && n.RoadmapId == node.RoadmapId, ct);
            if (entity is null)
                throw new InvalidOperationException("Node not found.");

            RoadmapNodeMapper.UpdateEntity(node, entity);

        }
        public async Task<RoadmapNode?> GetByIdAsync(string tenantId, string roadmapId, string nodeId, CancellationToken ct = default)
        {
            if (!TryNormalize(tenantId, roadmapId, nodeId, out var tId, out var rId, out var nId))
                return null;

            var entity = await _dbContext.Set<RoadmapNodeEntity>()
                .AsNoTracking()
                .Include(n => n.Roadmap) // tenant check
                .FirstOrDefaultAsync(n =>
                    n.Id == nId &&
                    n.RoadmapId == rId &&
                    n.Roadmap.TenantId == tId, ct);

            return entity is null ? null : RoadmapNodeMapper.ToDomain(entity);
        }

        public async Task<List<RoadmapNode>> GetByRoadmapIdAsync(string tenantId, string roadmapId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(roadmapId))
                return new();

            var tId = tenantId.Trim();
            var rId = roadmapId.Trim();

            var entities = await _dbContext.Set<RoadmapNodeEntity>()
                .AsNoTracking()
                .Where(n => n.RoadmapId == rId && n.Roadmap.TenantId == tId)
                .OrderBy(n => n.Order)
                .ToListAsync(ct);

            return entities.Select(RoadmapNodeMapper.ToDomain).ToList();
        }

        private static bool TryNormalize(
       string? tenantId,
       string? roadmapId,
       string? nodeId,
       out string tId,
       out string rId,
       out string nId)
        {
            tId = tenantId?.Trim() ?? "";
            rId = roadmapId?.Trim() ?? "";
            nId = nodeId?.Trim() ?? "";

            return !string.IsNullOrWhiteSpace(tId)
                && !string.IsNullOrWhiteSpace(rId)
                && !string.IsNullOrWhiteSpace(nId);
        }
    }
}
