using Application.Common;
using Application.Common.DTOs;
using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface IRoadmapsRepository
    {
        Task AddAsync(Roadmap roadmap, CancellationToken cancellationToken = default);
        Task UpdateAsync(Roadmap roadmap, CancellationToken cancellationToken = default);
        Task<bool> RemoveAsync(string tenantId, string roadmapId, CancellationToken cancellationToken = default);
        Task DeleteReferencingNodesAsync(
            string tenantId,
            string roadmapId,
            CancellationToken ct = default);

    Task<Roadmap?> GetByIdAsync(
        string tenantId,
        string roadmapId,
        bool includeHidden = false,
        CancellationToken ct = default);
    Task<Roadmap?> GetByIdAsync(
       string tenantId,
       string roadmapId,
       CancellationToken ct = default);
    Task AddTagsAsync(string tenantId, string roadmapId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default);
    Task RemoveTagsAsync(string tenantId, string roadmapId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default);
    Task SetTagsAsync(string tenantId, string roadmapId, IReadOnlyCollection<string> tagIds, CancellationToken cancellationToken = default);

    Task<bool> IsTitleUniqueAsync(string tenantId, string title, CancellationToken cancellationToken = default);

    Task<PagedList<Roadmap, DateTime?>> GetPagedAsync(
        string tenantId,
        QueryParametersDto<DateTime?> parameters,
        CancellationToken cancellationToken = default);
}
