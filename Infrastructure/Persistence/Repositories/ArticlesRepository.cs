using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ArticlesRepository : IArticlesRepository
    {
        private readonly AppDbContext _dbContext;
        public ArticlesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public async Task AddAsync(Article article, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(article);
            var entity = ArticleMapper.ToEntity(article);
            await _dbContext.Articles.AddAsync(entity, cancellationToken);
        }

        public async Task AddTagsAsync(string tenantId, string articleId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, articleId, out var tId, out var aId))
                throw new ArgumentException("Tenant ID and Article ID must be provided and cannot be whitespace.");
            var ids = NormalizeIds(tagIds);
            if (ids.Count == 0)
                return;
            var entity = await _dbContext.Articles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == aId && a.TenantId == tId, cancellationToken);
            if (entity is null)
                throw new InvalidOperationException("Article not found.");
            await EnsureTagsExistInTenantAsync(tId, ids, cancellationToken);
            var domain = ArticleMapper.ToDomain(entity);
            foreach (var id in ids)
            {
                domain.AddTag(id);
            }
            var existing = entity.Tags.Select(t => t.Id)
                .ToHashSet(StringComparer.Ordinal);
            var tagsToAdd = await _dbContext.Tags.Where(t => ids.Contains(t.Id) && !existing.Contains(t.Id)).ToListAsync(cancellationToken);
            foreach (var tag in tagsToAdd)
            {
                if (existing.Contains(tag.Id))
                    continue;
                entity.Tags.Add(tag);
            }
            ArticleMapper.UpdateEntity(domain, entity);

        }

        public async Task<bool> ExistsAsync(string tenantId, string articleId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, articleId, out var tId, out var aId))
                return false;
            return await _dbContext.Articles
                .AsNoTracking()
                .AnyAsync(a => a.Id == aId && a.TenantId == tId, cancellationToken);
        }

        public async Task<Article?> GetByIdAsync(string tenantId, string articleId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, articleId, out var tId, out var aId))
                return null;
            var entity = await _dbContext.Articles
                .AsNoTracking()
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == aId && a.TenantId == tId, cancellationToken);
            return entity is null ? null : ArticleMapper.ToDomain(entity);
        }

        public async Task<bool> RemoveAsync(string tenantId, string articleId, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, articleId, out var tId, out var aId))
                return false;
            var entity = _dbContext.Articles.FirstOrDefault(a => a.Id == aId && a.TenantId == tId);
            if (entity is null)
                return false;
            _dbContext.Articles.Remove(entity);
            return true;
        }

        public async Task RemoveTagsAsync(
       string tenantId,
       string articleId,
       IReadOnlyCollection<string> tagIds,
       CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, articleId, out var tId, out var aId))
                throw new ArgumentNullException("tenantId/articleId");

            var ids = NormalizeIds(tagIds);
            if (ids.Count == 0)
                return;

            var entity = await _dbContext.Articles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.TenantId == tId && a.Id == aId, cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Article not found.");

            var domain = ArticleMapper.ToDomain(entity);

            foreach (var id in ids)
                domain.RemoveTag(id);

            var toRemove = entity.Tags
                .Where(t => ids.Contains(t.Id, StringComparer.Ordinal))
                .ToList();

            foreach (var tag in toRemove)
                entity.Tags.Remove(tag);

            ArticleMapper.UpdateEntity(domain, entity);
        }

        public async Task SetTagsAsync(string tenantId, string articleId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default)
        {
            if (!TryNormalize(tenantId, articleId, out var tId, out var aId))
                throw new ArgumentNullException("tenantId/articleId");

            var ids = NormalizeIds(tagIds);
            var entity = await _dbContext.Articles
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == aId && a.TenantId == tId, cancellationToken);
            if (entity is null)
                throw new InvalidOperationException("Article not found.");
            if (ids.Count > 0)
                await EnsureTagsExistInTenantAsync(tId, ids, cancellationToken);
            var domain = ArticleMapper.ToDomain(entity);
            domain.SetTags(ids);
            var tags = ids.Count == 0
           ? new List<TagEntity>()
           : await _dbContext.Tags
               .Where(t => ids.Contains(t.Id) && t.TenantId == tId)
               .ToListAsync(cancellationToken);

            entity.Tags = tags;

            ArticleMapper.UpdateEntity(domain, entity);
        }


        public async Task UpdateAsync(Article article, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(article);
            var entity = await _dbContext.Articles
                .FirstOrDefaultAsync(a => a.Id == article.Id && a.TenantId == article.TenantId, cancellationToken);
            if (entity is null)
                throw new InvalidOperationException("Article not found.");
            ArticleMapper.UpdateEntity(article, entity);
        }
        private static bool TryNormalize(string? tenantId, string? articleId, out string normalizedTenantId, out string normalizedArticleId)
        {
            normalizedTenantId = tenantId?.Trim() ?? string.Empty;
            normalizedArticleId = articleId?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(normalizedTenantId) && !string.IsNullOrWhiteSpace(normalizedArticleId);
        }
        private List<string> NormalizeIds(IEnumerable<string>? ids)
        {
            if (ids is null || ids.Count() == 0)
                return new();
            return ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        }
        private async Task EnsureTagsExistInTenantAsync(
      string tenantId,
      List<string> tagIds,
      CancellationToken ct)
        {
            var tags = await _dbContext.Tags
                .AsNoTracking()
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => new { t.Id, t.TenantId })
                .ToListAsync(ct);

            if (tags.Count != tagIds.Count)
                throw new InvalidOperationException("One or more tags were not found.");

            if (tags.Any(t => t.TenantId != tenantId))
                throw new InvalidOperationException("One or more tags are not in this tenant.");
        }
    }
}
