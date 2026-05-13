using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IArticleLikesRepository
    {
        Task AddAsync(ArticleLike like, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            string articleId,
            string createdByUserId,
            CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(
            string articleId,
            string createdByUserId,
            CancellationToken cancellationToken = default);
    }
}
