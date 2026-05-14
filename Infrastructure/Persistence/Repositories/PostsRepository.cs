using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostsRepository : IPostsRepository
{
    private readonly AppDbContext _dbContext;

    public PostsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Post post, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(post);

        await _dbContext.Posts.AddAsync(PostMapper.ToEntity(post), ct);
    }

    public async Task<Post?> GetByIdAsync(
        string tenantId,
        string postId,
        CancellationToken ct = default)
    {
        if (!TryNormalize(tenantId, postId, out var tId, out var pId))
            return null;

        var entity = await _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Tags)
            .Include(x => x.Links)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == pId, ct);

        return entity is null ? null : PostMapper.ToDomain(entity);
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(post);

        var entity = await _dbContext.Posts
            .Include(x => x.Links)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == post.Id && x.TenantId == post.TenantId, ct);

        if (entity is null)
            throw new InvalidOperationException("Post not found.");

        PostMapper.UpdateEntity(entity, post);
    }

    public async Task<bool> DeleteAsync(
        string tenantId,
        string postId,
        CancellationToken ct = default)
    {
        if (!TryNormalize(tenantId, postId, out var tId, out var pId))
            return false;

        var entity = await _dbContext.Posts
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == pId, ct);

        if (entity is null)
            return false;

        _dbContext.Posts.Remove(entity);
        return true;
    }

    public Task<bool> ExistsAsync(
        string tenantId,
        string postId,
        CancellationToken ct = default)
    {
        return !TryNormalize(tenantId, postId, out var tId, out var pId)
            ? Task.FromResult(false)
            : _dbContext.Posts
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tId && x.Id == pId, ct);
    }

    public async Task AddTagsAsync(
        string tenantId,
        string postId,
        IReadOnlyCollection<string> tagIds,
        CancellationToken ct = default)
    {
        if (!TryNormalize(tenantId, postId, out var tId, out var pId))
            throw new ArgumentException("TenantId/PostId is required.");

        var ids = NormalizeIds(tagIds);
        if (ids.Count == 0)
            return;

        var entity = await _dbContext.Posts
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == pId, ct);

        if (entity is null)
            throw new InvalidOperationException("Post not found.");

        var currentIds = entity.Tags
            .Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var newIds = ids
            .Where(id => !currentIds.Contains(id))
            .ToList();

        if (currentIds.Count + newIds.Count > 5)
            throw new InvalidOperationException("A post can have maximum 5 tags.");

        if (newIds.Count == 0)
            return;

        var tags = await _dbContext.Tags
            .Where(x => newIds.Contains(x.Id) && x.TenantId == tId)
            .ToListAsync(ct);

        if (tags.Count != newIds.Count)
            throw new InvalidOperationException("One or more tags were not found.");

        foreach (var tag in tags)
            entity.Tags.Add(tag);
    }

    public async Task RemoveTagsAsync(
        string tenantId,
        string postId,
        IReadOnlyCollection<string> tagIds,
        CancellationToken ct = default)
    {
        if (!TryNormalize(tenantId, postId, out var tId, out var pId))
            throw new ArgumentException("TenantId/PostId is required.");

        var ids = NormalizeIds(tagIds);
        if (ids.Count == 0)
            return;

        var entity = await _dbContext.Posts
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == pId, ct);

        if (entity is null)
            throw new InvalidOperationException("Post not found.");

        var toRemove = entity.Tags
            .Where(x => ids.Contains(x.Id))
            .ToList();

        foreach (var tag in toRemove)
            entity.Tags.Remove(tag);
    }

    public async Task SetTagsAsync(
        string tenantId,
        string postId,
        IReadOnlyCollection<string> tagIds,
        CancellationToken ct = default)
    {
        if (!TryNormalize(tenantId, postId, out var tId, out var pId))
            throw new ArgumentException("TenantId/PostId is required.");

        var ids = NormalizeIds(tagIds);

        if (ids.Count > 5)
            throw new InvalidOperationException("A post can have maximum 5 tags.");

        var entity = await _dbContext.Posts
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.TenantId == tId && x.Id == pId, ct);

        if (entity is null)
            throw new InvalidOperationException("Post not found.");

        if (ids.Count == 0)
        {
            entity.Tags.Clear();
            return;
        }

        var tags = await _dbContext.Tags
            .Where(x => ids.Contains(x.Id) && x.TenantId == tId)
            .ToListAsync(ct);

        if (tags.Count != ids.Count)
            throw new InvalidOperationException("One or more tags were not found.");

        entity.Tags.Clear();

        foreach (var tag in tags)
            entity.Tags.Add(tag);
    }

    public Task<bool> HasUserLikedAsync(
        string postId,
        string userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(postId) || string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(false);

        var pId = postId.Trim();
        var uId = userId.Trim();

        return _dbContext.PostLikes
            .AsNoTracking()
            .AnyAsync(x => x.PostId == pId && x.CreatedByUserId == uId, ct);
    }

    public async Task AddLikeAsync(PostLike like, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(like);

        await _dbContext.PostLikes.AddAsync(PostLikeMapper.ToEntity(like), ct);
    }

    public async Task<bool> RemoveLikeAsync(
        string postId,
        string userId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(postId) || string.IsNullOrWhiteSpace(userId))
            return false;

        var pId = postId.Trim();
        var uId = userId.Trim();

        var entity = await _dbContext.PostLikes
            .FirstOrDefaultAsync(x => x.PostId == pId && x.CreatedByUserId == uId, ct);

        if (entity is null)
            return false;

        _dbContext.PostLikes.Remove(entity);
        return true;
    }

    public async Task AddCommentAsync(PostComment comment, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await _dbContext.PostComments.AddAsync(PostCommentMapper.ToEntity(comment), ct);
    }


    private static bool TryNormalize(
        string? tenantId,
        string? postId,
        out string tId,
        out string pId)
    {
        tId = tenantId?.Trim() ?? "";
        pId = postId?.Trim() ?? "";

        return !string.IsNullOrWhiteSpace(tId)
            && !string.IsNullOrWhiteSpace(pId);
    }

    private static List<string> NormalizeIds(IReadOnlyCollection<string>? ids)
    {
        return ids is null || ids.Count == 0
            ? new()
            : ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
