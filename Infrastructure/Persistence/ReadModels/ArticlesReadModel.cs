using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class ArticlesReadModel : IArticlesReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ArticlesReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ArticleDto?> GetByIdAsync(
            string tenantId,
            string articleId,
            string? currentUserId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(articleId))
                return null;

            var tId = tenantId.Trim();
            var aId = articleId.Trim();

            var dto = await _dbContext.Articles
                .AsNoTracking()
                .Where(a => a.TenantId == tId && a.Id == aId)
                .ProjectTo<ArticleDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto is null)
                return null;

            dto.IsLikedByCurrentUser = await IsLikedAsync(
                aId,
                currentUserId,
                cancellationToken);

            return dto;
        }

        public async Task<ArticleDetailsDto?> GetDetailsAsync(
            string tenantId,
            string articleId,
            string? currentUserId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(articleId))
                return null;

            var tId = tenantId.Trim();
            var aId = articleId.Trim();

            var dto = await _dbContext.Articles
                .AsNoTracking()
                .Where(a => a.TenantId == tId && a.Id == aId)
                .ProjectTo<ArticleDetailsDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            if (dto is null)
                return null;

            dto.IsLikedByCurrentUser = await IsLikedAsync(
                aId,
                currentUserId,
                cancellationToken);

            return dto;
        }

        public async Task<PagedList<ArticleDto, DateTime?>> GetArticlesAsync(
            ArticlesSearchAndFilterParametersDto parameters,
            string? currentUserId = null,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new ArticlesSearchAndFilterParametersDto();

            var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

            IQueryable<ArticleEntity> query = _dbContext.Articles.AsNoTracking();

            query = ApplyFilters(query, parameters);

            if (parameters.Cursor.HasValue)
                query = query.Where(a => a.CreatedAt < parameters.Cursor.Value);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .ProjectTo<ArticleDto>(_mapper.ConfigurationProvider)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;

            if (items.Count > pageSize)
            {
                nextCursor = items[^1].CreatedAt;
                items.RemoveAt(items.Count - 1);
            }

            await FillLikedByCurrentUserAsync(
                items,
                currentUserId,
                cancellationToken);

            return new PagedList<ArticleDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }

        private async Task<bool> IsLikedAsync(
            string articleId,
            string? currentUserId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                return false;

            var userId = currentUserId.Trim();

            return await _dbContext.ArticleLikes
                .AsNoTracking()
                .AnyAsync(l =>
                    l.ArticleId == articleId &&
                    l.CreatedByUserId == userId,
                    cancellationToken);
        }

        private async Task FillLikedByCurrentUserAsync(
            List<ArticleDto> items,
            string? currentUserId,
            CancellationToken cancellationToken)
        {
            if (items.Count == 0 || string.IsNullOrWhiteSpace(currentUserId))
                return;

            var userId = currentUserId.Trim();
            var articleIds = items.Select(x => x.Id).ToList();

            var likedArticleIds = await _dbContext.ArticleLikes
                .AsNoTracking()
                .Where(l =>
                    articleIds.Contains(l.ArticleId) &&
                    l.CreatedByUserId == userId)
                .Select(l => l.ArticleId)
                .ToListAsync(cancellationToken);

            var likedSet = likedArticleIds.ToHashSet(StringComparer.Ordinal);

            foreach (var item in items)
            {
                item.IsLikedByCurrentUser = likedSet.Contains(item.Id);
            }
        }
        public async Task<PagedList<ArticleLikeUserDto, DateTime?>> GetArticleLikesAsync(
    string tenantId,
    string articleId,
    int pageSize,
    DateTime? cursor,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(articleId))
            {
                return new PagedList<ArticleLikeUserDto, DateTime?>
                {
                    Items = new(),
                    NextCursor = null
                };
            }

            var tId = tenantId.Trim();
            var aId = articleId.Trim();

            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _dbContext.ArticleLikes
                .AsNoTracking()
                .Where(l =>
                    l.ArticleId == aId &&
                    l.Article.TenantId == tId);

            if (cursor.HasValue)
                query = query.Where(l => l.CreatedAt < cursor.Value);

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new ArticleLikeUserDto
                {
                    UserId = l.CreatedByUserId,
                    UserName = l.CreatedByUser != null ? l.CreatedByUser.FullName : "",
                    UserProfileImageUrl = l.CreatedByUser.ProfileImage != null
                        ? l.CreatedByUser.ProfileImage.Url
                        : null,
                    LikedAt = l.CreatedAt
                })
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;

            if (items.Count > pageSize)
            {
                nextCursor = items[^1].LikedAt;
                items.RemoveAt(items.Count - 1);
            }

            return new PagedList<ArticleLikeUserDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }
        private static IQueryable<ArticleEntity> ApplyFilters(
            IQueryable<ArticleEntity> query,
            ArticlesSearchAndFilterParametersDto p)
        {
            if (!string.IsNullOrWhiteSpace(p.TenantId))
            {
                var tenantId = p.TenantId.Trim();
                query = query.Where(a => a.TenantId == tenantId);
            }

            if (!string.IsNullOrWhiteSpace(p.Search))
            {
                var term = p.Search.Trim();

                query = query.Where(a =>
                    a.Title.Contains(term) ||
                    a.Content.Contains(term));
            }

            if (p.IsPublished.HasValue)
                query = query.Where(a => a.IsPublished == p.IsPublished.Value);

            if (!string.IsNullOrWhiteSpace(p.CreatedByUserId))
            {
                var userId = p.CreatedByUserId.Trim();
                query = query.Where(a => a.CreatedByUserId == userId);
            }

            if (p.CreatedAfter.HasValue)
                query = query.Where(a => a.CreatedAt >= p.CreatedAfter.Value);

            if (p.CreatedBefore.HasValue)
                query = query.Where(a => a.CreatedAt <= p.CreatedBefore.Value);

            if (p.UpdatedAfter.HasValue)
                query = query.Where(a => a.UpdatedAt != null && a.UpdatedAt >= p.UpdatedAfter.Value);

            if (p.UpdatedBefore.HasValue)
                query = query.Where(a => a.UpdatedAt != null && a.UpdatedAt <= p.UpdatedBefore.Value);

            if (p.TagIdsAny is { Count: > 0 })
            {
                var ids = NormalizeIds(p.TagIdsAny);

                if (ids.Count > 0)
                    query = query.Where(a => a.Tags.Any(t => ids.Contains(t.Id)));
            }

            if (p.TagIdsAll is { Count: > 0 })
            {
                var ids = NormalizeIds(p.TagIdsAll);

                if (ids.Count > 0)
                    query = query.Where(a => ids.All(tagId => a.Tags.Any(t => t.Id == tagId)));
            }

            return query;
        }

        private static List<string> NormalizeIds(IReadOnlyCollection<string>? ids)
        {
            if (ids is null || ids.Count == 0)
                return new();

            return ids
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
    }
}
