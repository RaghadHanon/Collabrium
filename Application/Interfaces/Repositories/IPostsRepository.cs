using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IPostsRepository
    {
        Task AddAsync(Post post, CancellationToken ct = default);

        Task<Post?> GetByIdAsync(
            string tenantId,
            string postId,
            CancellationToken ct = default);

        Task UpdateAsync(Post post, CancellationToken ct = default);

        Task<bool> DeleteAsync(
            string tenantId,
            string postId,
            CancellationToken ct = default);

        Task<bool> ExistsAsync(
            string tenantId,
            string postId,
            CancellationToken ct = default);

        Task AddTagsAsync(
            string tenantId,
            string postId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken ct = default);

        Task RemoveTagsAsync(
            string tenantId,
            string postId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken ct = default);

        Task SetTagsAsync(
            string tenantId,
            string postId,
            IReadOnlyCollection<string> tagIds,
            CancellationToken ct = default);

        Task<bool> HasUserLikedAsync(
            string postId,
            string userId,
            CancellationToken ct = default);

        Task AddLikeAsync(PostLike like, CancellationToken ct = default);

        Task<bool> RemoveLikeAsync(
            string postId,
            string userId,
            CancellationToken ct = default);


    }
}
