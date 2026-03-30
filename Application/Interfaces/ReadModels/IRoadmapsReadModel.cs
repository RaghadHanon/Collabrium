using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Features.Tags.DTOs;

namespace Application.Interfaces.ReadModels
{
    public interface IRoadmapsReadModel
    {
        Task<RoadmapDto?> GetByIdAsync(string tenantId, string roadmapId, CancellationToken cancellationToken = default);

        Task<PagedList<RoadmapDto, DateTime?>> GetRoadmapsAsync(
            RoadmapsSearchAndFilterParametersDto parameters,
            CancellationToken cancellationToken = default);

        Task<RoadmapDetailsDto?> GetDetailsAsync(
            string tenantId,
            string roadmapId,
            CancellationToken cancellationToken);
        Task<List<TagDto>> GetTagsAsync(
         string tenantId,
         string roadmapId,
         CancellationToken cancellationToken = default);

        Task<List<RoadmapDto>> GetOtherRoadmapsAsync(
           string tenantId,
           string currentRoadmapId,
           CancellationToken ct = default);
    }

}
