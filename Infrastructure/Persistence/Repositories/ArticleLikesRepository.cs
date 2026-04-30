using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public sealed class ArticleLikesRepository : IArticleLikesRepository
    {
        private readonly AppDbContext _dbContext;

        public ArticleLikesRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(ArticleLike like, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(like);

            var entity = ArticleLikeMapper.ToEntity(like);
            await _dbContext.ArticleLikes.AddAsync(entity, cancellationToken);
        }

        public Task<bool> ExistsAsync(
            string articleId,
            string createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(createdByUserId))
                return Task.FromResult(false);

            var aId = articleId.Trim();
            var userId = createdByUserId.Trim();

            return _dbContext.ArticleLikes
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ArticleId == aId &&
                    x.CreatedByUserId == userId,
                    cancellationToken);
        }

        public async Task<bool> RemoveAsync(
            string articleId,
            string createdByUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(articleId) || string.IsNullOrWhiteSpace(createdByUserId))
                return false;

            var aId = articleId.Trim();
            var userId = createdByUserId.Trim();

            var entity = await _dbContext.ArticleLikes
                .FirstOrDefaultAsync(x =>
                    x.ArticleId == aId &&
                    x.CreatedByUserId == userId,
                    cancellationToken);

            if (entity is null)
                return false;

            _dbContext.ArticleLikes.Remove(entity);
            return true;
        }
    }
}
