using Application.Common;
using Application.Features.Articles.DTOs;
using Application.Interfaces.ReadModels;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.ReadModels
{
    public sealed class ArticleCommentsReadModel : IArticleCommentsReadModel
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ArticleCommentsReadModel(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedList<ArticleCommentDto, DateTime?>> GetCommentsAsync(
            string articleId,
            int pageSize,
            DateTime? cursor,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                return new PagedList<ArticleCommentDto, DateTime?>
                {
                    Items = new(),
                    NextCursor = null
                };
            }

            var aId = articleId.Trim();
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _dbContext.ArticleComments
                .AsNoTracking()
                .Where(c => c.ArticleId == aId);

            if (cursor.HasValue)
                query = query.Where(c => c.CreatedAt < cursor.Value);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .ProjectTo<ArticleCommentDto>(_mapper.ConfigurationProvider)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            DateTime? nextCursor = null;

            if (items.Count > pageSize)
            {
                nextCursor = items[^1].CreatedAt;
                items.RemoveAt(items.Count - 1);
            }

            return new PagedList<ArticleCommentDto, DateTime?>
            {
                Items = items,
                NextCursor = nextCursor
            };
        }

        public Task<ArticleCommentDto?> GetByIdAsync(
            string articleId,
            string commentId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(commentId))
                return Task.FromResult<ArticleCommentDto?>(null);

            var aId = articleId.Trim();
            var cId = commentId.Trim();

            return _dbContext.ArticleComments
                .AsNoTracking()
                .Where(c => c.ArticleId == aId && c.Id == cId)
                .ProjectTo<ArticleCommentDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
