using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface ITagsRepository
    {
        Task AddAsync(Tag tag, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(string tenantId, string tagId, CancellationToken cancellationToken = default);
        Task<Tag?> GetByIdAsync(string tenantId, string tagId, CancellationToken cancellationToken = default);
    }
}
