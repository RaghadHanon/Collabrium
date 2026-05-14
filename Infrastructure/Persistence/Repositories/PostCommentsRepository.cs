using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PostCommentsRepository : IPostCommentsRepository
{
    private readonly AppDbContext _dbContext;

    public PostCommentsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(PostComment comment, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comment);

        await _dbContext.PostComments.AddAsync(
            PostCommentMapper.ToEntity(comment),
            ct);
    }

    public async Task<PostComment?> GetByIdAsync(
        string postId,
        string commentId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(postId) ||
            string.IsNullOrWhiteSpace(commentId))
            return null;

        var pId = postId.Trim();
        var cId = commentId.Trim();

        var entity = await _dbContext.PostComments
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.PostId == pId &&
                x.Id == cId,
                ct);

        return entity is null
            ? null
            : PostCommentMapper.ToDomain(entity);
    }

    public async Task UpdateAsync(PostComment comment, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var entity = await _dbContext.PostComments
            .FirstOrDefaultAsync(x =>
                x.Id == comment.Id &&
                x.PostId == comment.PostId,
                ct);

        if (entity is null)
            throw new InvalidOperationException("Comment not found.");

        PostCommentMapper.UpdateEntity(entity, comment);
    }

    public async Task<bool> DeleteAsync(
        string postId,
        string commentId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(postId) ||
            string.IsNullOrWhiteSpace(commentId))
            return false;

        var pId = postId.Trim();
        var cId = commentId.Trim();

        var entity = await _dbContext.PostComments
            .FirstOrDefaultAsync(x =>
                x.PostId == pId &&
                x.Id == cId,
                ct);

        if (entity is null)
            return false;

        _dbContext.PostComments.Remove(entity);

        return true;
    }
}
