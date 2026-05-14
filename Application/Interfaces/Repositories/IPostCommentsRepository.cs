using Domain.Models;

namespace Application.Interfaces.Repositories;

public interface IPostCommentsRepository
{
    Task AddAsync(PostComment comment, CancellationToken ct = default);

    Task<PostComment?> GetByIdAsync(
        string postId,
        string commentId,
        CancellationToken ct = default);

    Task UpdateAsync(PostComment comment, CancellationToken ct = default);

    Task<bool> DeleteAsync(
        string postId,
        string commentId,
        CancellationToken ct = default);
}
