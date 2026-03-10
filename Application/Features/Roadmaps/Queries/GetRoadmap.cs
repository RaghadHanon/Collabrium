using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Queries
{
    public class GetRoadmap
    {
        public class Query : IRequest<Result<RoadmapDetailsDto>>
        {
            public required string RoadmapId { get; set; }
        }
        public class Handler(ITenantContext tenantContext, IRoadmapsReadModel roadmapsReadModel)
       : IRequestHandler<Query, Result<RoadmapDetailsDto>>
        {
            public async Task<Result<RoadmapDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var tenantId = tenantContext.TenantId;

                if (string.IsNullOrWhiteSpace(tenantId))
                    return Result<RoadmapDetailsDto>.Failure("Tenant not found", HttpStatusCode.NotFound);

                var dto = await roadmapsReadModel.GetDetailsAsync(tenantId, request.RoadmapId, cancellationToken);
                return dto == null
                ? Result<RoadmapDetailsDto>.Failure("Roadmap not found", HttpStatusCode.NotFound)
                : Result<RoadmapDetailsDto>.Success(dto, HttpStatusCode.OK);

            }
        }
    }
}