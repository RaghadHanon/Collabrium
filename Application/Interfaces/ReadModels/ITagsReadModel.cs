using Application.Common;
using Application.Features.Tags.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface ITagsReadModel
    {
        Task<PagedList<TagDto, DateTime?>> GetAsync(
        TagsSearchParametersDto parameters,
        CancellationToken cancellationToken = default);

        Task<TagDto?> GetByIdAsync(
            string tenantId,
            string tagId,
            CancellationToken cancellationToken = default);
    }
}
