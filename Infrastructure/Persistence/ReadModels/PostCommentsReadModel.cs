using Application.Common;
using Application.Features.Posts.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels;

public sealed class PostCommentsReadModel : IPostCommentsReadModel
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public PostCommentsReadModel(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedList<PostCommentDto, DateTime?>> GetCommentsAsync(
        string tenantId,
        string postId,
        int pageSize,
        DateTime? cursor,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(postId))
        {
            return new PagedList<PostCommentDto, DateTime?>
            {
                Items = new(),
                NextCursor = null
            };
        }

        var tId = tenantId.Trim();
        var pId = postId.Trim();

        pageSize = pageSize <= 0 ? 10 : pageSize;

        var query = _dbContext.PostComments
            .AsNoTracking()
            .Where(x =>
                x.PostId == pId &&
                x.Post.TenantId == tId);

        if (cursor.HasValue)
            query = query.Where(x => x.CreatedAt < cursor.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<PostCommentDto>(_mapper.ConfigurationProvider)
            .Take(pageSize + 1)
            .ToListAsync(ct);

        DateTime? nextCursor = null;

        if (items.Count > pageSize)
        {
            nextCursor = items[^1].CreatedAt;
            items.RemoveAt(items.Count - 1);
        }

        return new PagedList<PostCommentDto, DateTime?>
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public Task<PostCommentDto?> GetByIdAsync(
        string tenantId,
        string postId,
        string commentId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(postId) ||
            string.IsNullOrWhiteSpace(commentId))
        {
            return Task.FromResult<PostCommentDto?>(null);
        }

        var tId = tenantId.Trim();
        var pId = postId.Trim();
        var cId = commentId.Trim();

        return _dbContext.PostComments
            .AsNoTracking()
            .Where(x =>
                x.PostId == pId &&
                x.Id == cId &&
                x.Post.TenantId == tId)
            .ProjectTo<PostCommentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }
}
