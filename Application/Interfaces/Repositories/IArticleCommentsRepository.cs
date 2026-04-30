using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IArticleCommentsRepository
    {
        Task AddAsync(ArticleComment comment, CancellationToken cancellationToken = default);

        Task<ArticleComment?> GetByIdAsync(
            string articleId,
            string commentId,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(ArticleComment comment, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(
            string articleId,
            string commentId,
            CancellationToken cancellationToken = default);
    }
}
