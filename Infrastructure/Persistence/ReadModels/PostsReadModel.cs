using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class PostsReadModel : IPostsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public PostsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedList<PostDto, DateTime?>> GetPostsAsync(
        PostsSearchAndFilterParametersDto parameters,
        CancellationToken ct = default)
    {
        parameters ??= new PostsSearchAndFilterParametersDto();

        var pageSize = parameters.PageSize <= 0 ? 10 : parameters.PageSize;

        var query = _dbContext.Posts.AsNoTracking();

        query = ApplyFilters(query, parameters);

        if (parameters.Cursor.HasValue)
            query = query.Where(x => x.CreatedAt < parameters.Cursor.Value);

        var currentUserId = parameters.CurrentUserId?.Trim();

        var items = await query
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.Likes.Count + x.Comments.Count)
            .ThenByDescending(x => x.CreatedAt)
            .ProjectTo<PostDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<PostDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public Task<PostDto?> GetByIdAsync(
        string tenantId,
        string postId,
        string? currentUserId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(postId))
            return Task.FromResult<PostDto?>(null);

        var tId = tenantId.Trim();
        var pId = postId.Trim();
        currentUserId = currentUserId?.Trim();

        return _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.Id == pId)
            .ProjectTo<PostDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .FirstOrDefaultAsync(ct);
    }

    public Task<PostDetailsDto?> GetDetailsAsync(
        string tenantId,
        string postId,
        string? currentUserId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) || string.IsNullOrWhiteSpace(postId))
            return Task.FromResult<PostDetailsDto?>(null);

        var tId = tenantId.Trim();
        var pId = postId.Trim();
        currentUserId = currentUserId?.Trim();

        return _dbContext.Posts
            .AsNoTracking()
            .Where(x => x.TenantId == tId && x.Id == pId)
            .ProjectTo<PostDetailsDto>(
                _mapper.ConfigurationProvider,
                new { currentUserId })
            .FirstOrDefaultAsync(ct);
    }

    private static IQueryable<PostEntity> ApplyFilters(
        IQueryable<PostEntity> query,
        PostsSearchAndFilterParametersDto p)
    {
        if (!string.IsNullOrWhiteSpace(p.TenantId))
        {
            var tenantId = p.TenantId.Trim();
            query = query.Where(x => x.TenantId == tenantId);
        }

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(term) ||
                x.Content.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(p.CreatedByUserId))
        {
            var userId = p.CreatedByUserId.Trim();
            query = query.Where(x => x.CreatedByUserId == userId);
        }

        if (p.Type.HasValue)
            query = query.Where(x => x.Type == p.Type.Value);

        if (p.IsPinned.HasValue)
            query = query.Where(x => x.IsPinned == p.IsPinned.Value);

        if (p.HasImages.HasValue)
        {
            query = p.HasImages.Value
                ? query.Where(x => x.Images.Any())
                : query.Where(x => !x.Images.Any());
        }

        if (p.HasLinks.HasValue)
        {
            query = p.HasLinks.Value
                ? query.Where(x => x.Links.Any())
                : query.Where(x => !x.Links.Any());
        }

        if (p.TagIdsAny is { Count: > 0 })
        {
            var ids = p.TagIdsAny
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            if (ids.Count > 0)
                query = query.Where(x => x.Tags.Any(t => ids.Contains(t.Id)));
        }

        if (p.TagIdsAll is { Count: > 0 })
        {
            var ids = p.TagIdsAll
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            if (ids.Count > 0)
                query = query.Where(x =>
                    ids.All(tagId => x.Tags.Any(t => t.Id == tagId)));
        }

        if (p.CreatedAfter.HasValue)
            query = query.Where(x => x.CreatedAt >= p.CreatedAfter.Value);

        if (p.CreatedBefore.HasValue)
            query = query.Where(x => x.CreatedAt <= p.CreatedBefore.Value);

        if (p.UpdatedAfter.HasValue)
            query = query.Where(x =>
                x.UpdatedAt != null &&
                x.UpdatedAt >= p.UpdatedAfter.Value);

        if (p.UpdatedBefore.HasValue)
            query = query.Where(x =>
                x.UpdatedAt != null &&
                x.UpdatedAt <= p.UpdatedBefore.Value);

        return query;
    }
}