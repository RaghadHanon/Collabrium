using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    internal class ArticleCommentsRepository : IArticleCommentsRepository
    {
        private readonly AppDbContext _dbContext;
        public ArticleCommentsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public async Task AddAsync(ArticleComment comment, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(comment);
            var entity = ArticleCommentMapper.ToEntity(comment);
            await _dbContext.ArticleComments.AddAsync(entity, cancellationToken);
        }

        public async Task<ArticleComment?> GetByIdAsync(string articleId, string commentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(commentId))
                return null;
            var aId = articleId.Trim();
            var cId = commentId.Trim();
            var entity = await _dbContext.ArticleComments
                 .AsNoTracking()
                 .FirstOrDefaultAsync(c => c.Id == cId &&
                 c.ArticleId == aId, cancellationToken);

            return entity is null ? null : ArticleCommentMapper.ToDomain(entity);

        }

        public async Task<bool> RemoveAsync(string articleId, string commentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(commentId))
                return false;
            var aId = articleId.Trim();
            var cId = commentId.Trim();
            var entity = await _dbContext.ArticleComments
                 .FirstOrDefaultAsync(c => c.Id == cId &&
                 c.ArticleId == aId, cancellationToken);
            if (entity is null)
                return false;
            _dbContext.ArticleComments.Remove(entity);
            return true;

        }

        public async Task UpdateAsync(ArticleComment comment, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(comment);

            var entity = await _dbContext.ArticleComments
                .FirstOrDefaultAsync(x =>
                    x.Id == comment.Id &&
                    x.ArticleId == comment.ArticleId,
                    cancellationToken);

            if (entity is null)
                throw new InvalidOperationException("Comment not found.");

            ArticleCommentMapper.UpdateEntity(comment, entity);
        }
    }
}
