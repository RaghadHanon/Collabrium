using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IArticlesRepository
    {
        Task AddAsync(
            Article article,
            CancellationToken cancellationToken = default);
        Task UpdateAsync(
            Article article,
            CancellationToken cancellationToken = default);
        Task<Article?> GetByIdAsync(
            string tenantId,
            string articleId,
            CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(
            string tenantId,
            string articleId,
            CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(
            string tenantId,
            string articleId,
            CancellationToken cancellationToken = default);
        Task AddTagsAsync(
            string tenantId,
            string articleId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken cancellationToken = default);
        Task RemoveTagsAsync(
            string tenantId,
            string articleId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken cancellationToken = default);
        Task SetTagsAsync(
            string tenantId,
            string articleId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken cancellationToken = default);

    }
}
