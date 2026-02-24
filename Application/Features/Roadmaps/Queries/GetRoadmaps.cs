using Application.Common;
using Application.Features.Roadmaps.DTOs;
using Application.Interfaces;
using Application.Interfaces.ReadModels;
using AutoMapper;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Roadmaps.Queries
{
    public class GetRoadmaps
    {
        public class Query : IRequest<Result<PagedList<RoadmapDto, DateTime?>>>
        {
            public string? TitleContains { get; set; }
            public string? DescriptionContains { get; set; }

            public RoadmapPath? Path { get; set; }
            public RoadmapLevel? Level { get; set; }


            public bool? IsVisible { get; set; }

            public DateTime? PublishedAfter { get; set; }
            public DateTime? PublishedBefore { get; set; }

            public DateTime? UpdatedAfter { get; set; }
            public DateTime? UpdatedBefore { get; set; }

            public DateTime? CreatedAfter { get; set; }
            public DateTime? CreatedBefore { get; set; }

            public DateTime? Cursor { get; set; }
            public int PageSize { get; set; } = 10;

            public IReadOnlyCollection<string>? TagIdsAny { get; set; }
            public IReadOnlyCollection<string>? TagIdsAll { get; set; }
        }
        public class Handler(
    IRoadmapsReadModel roadmapsReadModel,
    ITenantContext tenantContext,
    IMapper mapper
) : IRequestHandler<Query, Result<PagedList<RoadmapDto, DateTime?>>>
        {
            public async Task<Result<PagedList<RoadmapDto, DateTime?>>> Handle(
                Query request,
                CancellationToken cancellationToken)
            {
                var tenant = tenantContext.CurrentTenant;
                if (tenant is null)
                {
                    return Result<PagedList<RoadmapDto, DateTime?>>.Failure(
                        "Tenant not found.",
                        HttpStatusCode.NotFound);
                }

                var parameters =
                    mapper.Map<RoadmapsSearchAndFilterParametersDto>(request);

                parameters.TenantId = tenant.Id;

                var pagedRoadmaps =
                    await roadmapsReadModel.GetRoadmapsAsync(
                        parameters,
                        cancellationToken);

                return Result<PagedList<RoadmapDto, DateTime?>>.Success(
                    pagedRoadmaps,
                    HttpStatusCode.OK);
            }
        }
    }

}

