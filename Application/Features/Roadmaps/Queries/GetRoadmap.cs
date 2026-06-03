using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using Application.Interfaces.Services;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Queries;

public class GetRoadmap
{
    public class Query : IRequest<Result<RoadmapDetailsDto>>
    {
        public required string RoadmapId { get; set; }
    }
    public class Handler(
     ITenantContext tenantContext,
     IRoadmapsReadModel roadmapsReadModel,
     ICurrentUserAccessor currentUserAccessor)
     : IRequestHandler<Query, Result<RoadmapDetailsDto>>
    {
        public async Task<Result<RoadmapDetailsDto>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var tenantId = tenantContext.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Result<RoadmapDetailsDto>.Failure(
                    "Tenant not found",
                    HttpStatusCode.NotFound);

            var dto = await roadmapsReadModel.GetDetailsAsync(
                tenantId,
                request.RoadmapId,
                cancellationToken);

            if (dto is null)
                return Result<RoadmapDetailsDto>.Failure(
                    "Roadmap not found",
                    HttpStatusCode.NotFound);

            var canViewHidden =
                await RoadmapAccessHelper.CanCurrentUserViewHiddenAsync(
                    currentUserAccessor,
                    cancellationToken);

            if (!dto.IsVisible && !canViewHidden)
                return Result<RoadmapDetailsDto>.Failure(
                    "Roadmap not found",
                    HttpStatusCode.NotFound);

            return Result<RoadmapDetailsDto>.Success(
                dto,
                HttpStatusCode.OK);
        }
    }
}