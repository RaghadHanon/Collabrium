using Application.Common;
using Application.Common.DTOs;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class RoadmapsRepository : IRoadmapsRepository
    {
        private readonly AppDbContext _dbContext;

        public RoadmapsRepository(AppDbContext dbContext)
            => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task AddAsync(Roadmap roadmap, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(roadmap);
            var entity = RoadmapMapper.ToEntity(roadmap);
            await _dbContext.Roadmaps.AddAsync(entity, cancellationToken);
        }

        public async Task UpdateAsync(Roadmap roadmap, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(roadmap);

            var entity = await _dbContext.Roadmaps
                .FirstOrDefaultAsync(r => r.Id == roadmap.Id, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Roadmap not found.");

            RoadmapMapper.UpdateEntity(roadmap, entity);
        }

        public async Task<bool> RemoveAsync(string tenantId, string roadmapId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, roadmapId, out var tId, out var rId))
                return false;

            var entity = await _dbContext.Roadmaps
                .FirstOrDefaultAsync(r => r.TenantId == tId && r.Id == rId, cancellationToken);

            if (entity is null) return false;

            _dbContext.Roadmaps.Remove(entity);
            return true;
        }

        public async Task<Roadmap?> GetByIdAsync(string tenantId, string roadmapId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, roadmapId, out var tId, out var rId))
                return null;

            var entity = await _dbContext.Roadmaps
                .AsNoTracking()
                .Include(r => r.Tags)
                .FirstOrDefaultAsync(r => r.TenantId == tId && r.Id == rId, cancellationToken);

            return entity is null ? null : RoadmapMapper.ToDomain(entity);
        }
        public async Task<Roadmap?> GetByIdAsync(
            string tenantId,
            string roadmapId,
            bool includeHidden = false,
            CancellationToken ct = default)
        {

            if (!TryNormalize(tenantId, roadmapId, out var tId, out var rId))
                return null;

            IQueryable<RoadmapEntity> query = _dbContext.Roadmaps
                .AsNoTracking()
                .Where(r => r.TenantId == tenantId && r.Id == roadmapId);

            if (!includeHidden)
                query = query.Where(r => r.IsVisible);

            var entity = await query.FirstOrDefaultAsync(ct);

            return entity is null ? null : RoadmapMapper.ToDomain(entity);
        }

        public async Task AddTagsAsync(string tenantId, string roadmapId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, roadmapId, out var tId, out var rId))
                throw new ArgumentNullException("tenantId/roadmapId");

            var ids = NormalizeIds(tagIds);
            if (ids.Count == 0) return;

            var roadmapEntity = await _dbContext.Roadmaps
                .Include(r => r.Tags)
                .FirstOrDefaultAsync(r => r.TenantId == tId && r.Id == rId, cancellationToken);

            if (roadmapEntity is null)
                throw new InvalidOperationException("Roadmap not found.");

            await EnsureTagsExistInTenantAsync(tId, ids, cancellationToken);

            var domain = RoadmapMapper.ToDomain(roadmapEntity);
            foreach (var id in ids)
                domain.AddTag(id);

            var existing = roadmapEntity.Tags
                .Select(x => x.TagId)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var id in ids)
            {
                if (existing.Contains(id)) continue;

                roadmapEntity.Tags.Add(new RoadmapTagEntity
                {
                    RoadmapId = roadmapEntity.Id,
                    TagId = id
                });
            }

            RoadmapMapper.UpdateEntity(domain, roadmapEntity);
        }

        public async Task RemoveTagsAsync(string tenantId, string roadmapId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, roadmapId, out var tId, out var rId))
                throw new ArgumentNullException("tenantId/roadmapId");

            var ids = NormalizeIds(tagIds);
            if (ids.Count == 0) return;

            var roadmapEntity = await _dbContext.Roadmaps
                .Include(r => r.Tags)
                .FirstOrDefaultAsync(r => r.TenantId == tId && r.Id == rId, cancellationToken);

            if (roadmapEntity is null)
                throw new InvalidOperationException("Roadmap not found.");

            var domain = RoadmapMapper.ToDomain(roadmapEntity);
            foreach (var id in ids)
                domain.RemoveTag(id);

            var toRemove = roadmapEntity.Tags
                .Where(x => ids.Contains(x.TagId, StringComparer.Ordinal))
                .ToList();

            foreach (var row in toRemove)
                roadmapEntity.Tags.Remove(row);

            RoadmapMapper.UpdateEntity(domain, roadmapEntity);
        }

        public async Task SetTagsAsync(string tenantId, string roadmapId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, roadmapId, out var tId, out var rId))
                throw new ArgumentNullException("tenantId/roadmapId");

            var ids = NormalizeIds(tagIds);

            var roadmapEntity = await _dbContext.Roadmaps
                .Include(r => r.Tags)
                .FirstOrDefaultAsync(r => r.TenantId == tId && r.Id == rId, cancellationToken);

            if (roadmapEntity is null)
                throw new InvalidOperationException("Roadmap not found.");

            if (ids.Count > 0)
                await EnsureTagsExistInTenantAsync(tId, ids, cancellationToken);

            var domain = RoadmapMapper.ToDomain(roadmapEntity);
            domain.SetTags(ids);

            var desired = new HashSet<string>(ids, StringComparer.Ordinal);
            roadmapEntity.Tags = roadmapEntity.Tags.Where(x => desired.Contains(x.TagId)).ToList();

            var existing = roadmapEntity.Tags.Select(x => x.TagId).ToHashSet(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (existing.Contains(id)) continue;
                roadmapEntity.Tags.Add(new RoadmapTagEntity { RoadmapId = roadmapEntity.Id, TagId = id });
            }

            RoadmapMapper.UpdateEntity(domain, roadmapEntity);
        }

        public async Task<bool> IsTitleUniqueAsync(string tenantId, string title, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId)) return false;
            if (string.IsNullOrWhiteSpace(title)) return false;

            var tId = tenantId.Trim();
            var normalized = title.Trim().ToLowerInvariant();

            return !await _dbContext.Roadmaps
                .AsNoTracking()
                .AnyAsync(r => r.TenantId == tId && r.Title.ToLower() == normalized, cancellationToken);
        }

        public async Task<PagedList<Roadmap, DateTime?>> GetPagedAsync(
            string tenantId,
            QueryParametersDto<DateTime?> parameters,
            CancellationToken cancellationToken = default)
        {
            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

            if (string.IsNullOrWhiteSpace(tenantId))
                return new PagedList<Roadmap, DateTime?> { Items = new(), NextCursor = null };

            var tId = tenantId.Trim();

            var query = _dbContext.Roadmaps.AsNoTracking()
                .Where(r => r.TenantId == tId);

            if (parameters.Cursor.HasValue)
                query = query.Where(r => r.CreatedAt > parameters.Cursor.Value);

            var items = await query
                .OrderBy(r => r.CreatedAt)
                .Include(r => r.Tags) 
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;
            var hasMore = items.Count > pageSize;

            if (hasMore)
            {
                items.RemoveAt(items.Count - 1);
                nextCursor = items[^1].CreatedAt;
            }

            return new PagedList<Roadmap, DateTime?>
            {
                Items = items.Select(RoadmapMapper.ToDomain).ToList(),
                NextCursor = nextCursor
            };
        }

        private static bool TryNormalize(string? tenantId, string? roadmapId, out string tId, out string rId)
        {
            tId = tenantId?.Trim() ?? "";
            rId = roadmapId?.Trim() ?? "";
            return !string.IsNullOrWhiteSpace(tId) && !string.IsNullOrWhiteSpace(rId);
        }

        private static List<string> NormalizeIds(IReadOnlyCollection<string>? ids)
        {
            if (ids is null || ids.Count == 0) return new();

            return ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private async Task EnsureTagsExistInTenantAsync(string tenantId, List<string> tagIds, CancellationToken ct)
        {
            var tags = await _dbContext.Tags
                .AsNoTracking()
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => new { t.Id, t.TenantId })
                .ToListAsync(ct);

            if (tags.Count != tagIds.Count)
                throw new InvalidOperationException("One or more tags were not found.");

            if (tags.Any(t => !string.Equals(t.TenantId, tenantId, StringComparison.Ordinal)))
                throw new InvalidOperationException("One or more tags are not in the current tenant.");
        }
    }
}
