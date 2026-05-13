using Application.Common;
using Application.Features.Tags.DTOs;

namespace Application.Interfaces.ReadModels;

public interface ITagsReadModel
{
    Task<PagedList<TagDetailsDto, DateTime?>> GetAsync(
    TagsSearchAndFilterParametersDto parameters,
    CancellationToken cancellationToken = default);

    Task<TagDetailsDto?> GetByIdAsync(
        string tenantId,
        string tagId,
        CancellationToken cancellationToken = default);
}
